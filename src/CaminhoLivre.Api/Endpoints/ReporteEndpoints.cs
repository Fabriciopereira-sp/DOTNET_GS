using System.Security.Claims;
using System.Text;
using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;
using CaminhoLivre.Api.Infra;
using CaminhoLivre.Api.Services;
using CaminhoLivre.Api.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CaminhoLivre.Api.Endpoints;

public static class ReporteEndpoints
{
    public static IEndpointRouteBuilder MapReporteEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/reportes")
            .WithTags("Reportes do cidadão")
            .WithOpenApi()
            .RequireAuthorization();

        grupo.MapPost("/", async (
                CriarReporteRequest request,
                [FromHeader(Name = IdempotenciaService.Cabecalho)] string? chaveIdempotencia,
                ClaimsPrincipal user,
                HttpContext http,
                ReporteService service,
                IdempotenciaService idempotencia) =>
            {
                if (string.IsNullOrWhiteSpace(chaveIdempotencia) || chaveIdempotencia.Trim().Length > 100)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        [IdempotenciaService.Cabecalho] = ["Envie o cabeçalho Idempotency-Key (um UUID gerado pelo app), com até 100 caracteres."]
                    });
                }

                var erros = ReporteValidator.Validar(request);
                if (erros.Count > 0)
                {
                    return Results.ValidationProblem(erros);
                }

                var usuarioId = user.ObterUsuarioId();
                if (await service.BuscarUsuarioAsync(usuarioId) is null)
                {
                    return Results.Unauthorized();
                }

                var categoria = await service.BuscarCategoriaAsync(request.Categoria!);
                if (categoria is null)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["categoria"] = ["Categoria não encontrada. Consulte GET /api/categorias."]
                    });
                }

                var hash = IdempotenciaService.CalcularHash(request);
                var reserva = await idempotencia.ReservarAsync(chaveIdempotencia.Trim(), usuarioId, hash);

                switch (reserva.Situacao)
                {
                    case SituacaoIdempotencia.Repetida:
                        http.Response.Headers["Idempotent-Replayed"] = "true";
                        return Results.Content(reserva.Registro!.RespostaJson, "application/json", Encoding.UTF8, reserva.Registro.StatusHttp);

                    case SituacaoIdempotencia.CorpoDiferente:
                        return Results.Problem(
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "Chave de idempotência já usada",
                            detail: "Esta Idempotency-Key já foi usada com outro conteúdo. Gere uma nova chave para um novo reporte.");

                    case SituacaoIdempotencia.EmProcessamento:
                        return Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Requisição em processamento",
                            detail: "Uma requisição com esta Idempotency-Key ainda está sendo processada.");
                }

                try
                {
                    var reporte = await service.CriarAsync(usuarioId, categoria, request);
                    var resposta = ReporteMapper.ParaResponse(reporte);
                    await idempotencia.ConcluirAsync(reserva.Registro!, StatusCodes.Status201Created, resposta);
                    return Results.Created($"/api/reportes/{reporte.Id}", resposta);
                }
                catch
                {
                    await idempotencia.LiberarAsync(reserva.Registro!);
                    throw;
                }
            })
            .RequireAuthorization(Politicas.Cidadao)
            .RequireRateLimiting(LimiteDeRequisicoesExtensions.PoliticaCriacaoReporte)
            .WithName("CriarReporte")
            .WithSummary("Registra um novo reporte. Exige o cabeçalho Idempotency-Key: repetir a mesma requisição devolve o mesmo reporte")
            .Produces<ReporteResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        grupo.MapGet("/meus", async (ClaimsPrincipal user, ReporteService service, int page = 0, int size = 10) =>
            {
                var erros = ReporteValidator.ValidarPaginacao(page, size);
                if (erros.Count > 0)
                {
                    return Results.ValidationProblem(erros);
                }

                var pagina = await service.ListarDoUsuarioAsync(user.ObterUsuarioId(), page, size);
                return Results.Ok(pagina);
            })
            .RequireAuthorization(Politicas.Cidadao)
            .WithName("ListarMeusReportes")
            .WithSummary("Lista os reportes do cidadão logado, do mais recente para o mais antigo")
            .Produces<PaginaResponse<ReporteResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        grupo.MapGet("/{id:long}", async (long id, ClaimsPrincipal user, ReporteService service) =>
            {
                var reporte = await service.BuscarCompletoAsync(id);
                if (reporte is null || !await PodeVerAsync(reporte, user, service))
                {
                    return Results.NotFound();
                }

                return Results.Ok(ReporteMapper.ParaDetalhe(reporte));
            })
            .WithName("DetalharReporte")
            .WithSummary("Mostra o reporte com o histórico de status (dono do reporte ou gestor do órgão)")
            .Produces<ReporteDetalheResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapPut("/{id:long}", async (long id, AtualizarReporteRequest request, ClaimsPrincipal user, ReporteService service) =>
            {
                var erros = ReporteValidator.Validar(request);
                if (erros.Count > 0)
                {
                    return Results.ValidationProblem(erros);
                }

                var reporte = await service.BuscarCompletoAsync(id);
                if (reporte is null || reporte.UsuarioId != user.ObterUsuarioId())
                {
                    return Results.NotFound();
                }

                if (!RegrasReporte.PodeSerEditado(reporte.Status))
                {
                    return Conflito("O reporte só pode ser editado enquanto estiver ABERTO.");
                }

                await service.AtualizarAsync(reporte, request);
                return Results.Ok(ReporteMapper.ParaResponse(reporte));
            })
            .RequireAuthorization(Politicas.Cidadao)
            .WithName("EditarReporte")
            .WithSummary("Edita descrição e endereço do próprio reporte enquanto estiver ABERTO")
            .Produces<ReporteResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:long}", async (long id, ClaimsPrincipal user, ReporteService service) =>
            {
                var usuarioId = user.ObterUsuarioId();
                var reporte = await service.BuscarCompletoAsync(id);
                if (reporte is null || reporte.UsuarioId != usuarioId)
                {
                    return Results.NotFound();
                }

                if (!RegrasReporte.PodeMudarStatus(reporte.Status, StatusReporte.CANCELADO))
                {
                    return Conflito("O reporte só pode ser cancelado enquanto estiver ABERTO.");
                }

                await service.CancelarAsync(reporte, usuarioId);
                return Results.NoContent();
            })
            .RequireAuthorization(Politicas.Cidadao)
            .WithName("CancelarReporte")
            .WithSummary("Cancela o próprio reporte (status CANCELADO) enquanto estiver ABERTO")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    // Reporte de outra pessoa responde 404 (e não 403) para não revelar que o ID existe.
    private static async Task<bool> PodeVerAsync(Reporte reporte, ClaimsPrincipal user, ReporteService service)
    {
        var usuarioId = user.ObterUsuarioId();

        if (user.IsInRole(Papeis.Cidadao))
        {
            return reporte.UsuarioId == usuarioId;
        }

        if (user.IsInRole(Papeis.Gestor))
        {
            var gestor = await service.BuscarUsuarioAsync(usuarioId);
            return gestor?.OrgaoId == reporte.OrgaoId;
        }

        return false;
    }

    private static IResult Conflito(string mensagem) =>
        Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Operação não permitida", detail: mensagem);
}
