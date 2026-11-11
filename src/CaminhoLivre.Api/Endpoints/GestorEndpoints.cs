using System.Security.Claims;
using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;
using CaminhoLivre.Api.Services;
using CaminhoLivre.Api.Validation;

namespace CaminhoLivre.Api.Endpoints;

public static class GestorEndpoints
{
    public static IEndpointRouteBuilder MapGestorEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/gestor/reportes")
            .WithTags("Painel do gestor")
            .WithOpenApi()
            .RequireAuthorization(Politicas.Gestor);

        grupo.MapGet("/", async (
                ClaimsPrincipal user,
                GestorService service,
                string? status,
                string? urgencia,
                string? categoria,
                int page = 0,
                int size = 10) =>
            {
                var erros = ReporteValidator.ValidarPaginacao(page, size);

                StatusReporte? filtroStatus = null;
                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (TentarLerEnum<StatusReporte>(status, out var valor)) filtroStatus = valor;
                    else erros["status"] = ["Status inválido. Use ABERTO, EM_ANALISE, EM_EXECUCAO, RESOLVIDO ou CANCELADO."];
                }

                Urgencia? filtroUrgencia = null;
                if (!string.IsNullOrWhiteSpace(urgencia))
                {
                    if (TentarLerEnum<Urgencia>(urgencia, out var valor)) filtroUrgencia = valor;
                    else erros["urgencia"] = ["Urgência inválida. Use BAIXA, MEDIA ou ALTA."];
                }

                if (erros.Count > 0)
                {
                    return Results.ValidationProblem(erros);
                }

                var orgaoId = await service.BuscarOrgaoDoGestorAsync(user.ObterUsuarioId());
                if (orgaoId is null)
                {
                    return Results.Forbid();
                }

                var fila = await service.ListarFilaAsync(orgaoId.Value, filtroStatus, filtroUrgencia, categoria, page, size);
                return Results.Ok(fila);
            })
            .WithName("ListarFilaDoGestor")
            .WithSummary("Fila de reportes do órgão do gestor, ordenada por prioridade, com filtros opcionais")
            .Produces<PaginaResponse<ReporteResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        grupo.MapPatch("/{id:long}/status", async (long id, MudarStatusRequest request, ClaimsPrincipal user, GestorService service) =>
            {
                var erros = new Dictionary<string, string[]>();

                if (!TentarLerEnum<StatusReporte>(request.Status, out var novoStatus))
                {
                    erros["status"] = ["Informe o novo status: EM_ANALISE, EM_EXECUCAO ou RESOLVIDO."];
                }

                if (request.Observacao?.Length > 300)
                {
                    erros["observacao"] = ["A observação pode ter no máximo 300 caracteres."];
                }

                if (erros.Count > 0)
                {
                    return Results.ValidationProblem(erros);
                }

                var gestorId = user.ObterUsuarioId();
                var orgaoId = await service.BuscarOrgaoDoGestorAsync(gestorId);
                if (orgaoId is null)
                {
                    return Results.Forbid();
                }

                var reporte = await service.BuscarDoOrgaoAsync(id, orgaoId.Value);
                if (reporte is null)
                {
                    return Results.NotFound();
                }

                // Cancelamento é uma ação do cidadão; o gestor segue o fluxo de atendimento.
                if (novoStatus == StatusReporte.CANCELADO || !RegrasReporte.PodeMudarStatus(reporte.Status, novoStatus))
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Transição de status inválida",
                        detail: $"Não é possível mudar de {reporte.Status} para {novoStatus}.");
                }

                await service.MudarStatusAsync(reporte, novoStatus, request.Observacao, gestorId);
                return Results.Ok(ReporteMapper.ParaDetalhe(reporte));
            })
            .WithName("MudarStatusDoReporte")
            .WithSummary("Muda o status do reporte seguindo o fluxo ABERTO, EM_ANALISE, EM_EXECUCAO, RESOLVIDO")
            .Produces<ReporteDetalheResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static bool TentarLerEnum<T>(string? texto, out T valor) where T : struct, Enum =>
        Enum.TryParse(texto?.Trim(), ignoreCase: true, out valor)
        && !int.TryParse(texto, out _)
        && Enum.IsDefined(valor);
}
