using System.Security.Claims;
using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Services;
using CaminhoLivre.Api.Validation;

namespace CaminhoLivre.Api.Endpoints;

public static class ReporteEndpoints
{
    public static IEndpointRouteBuilder MapReporteEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/reportes")
            .WithTags("Reportes do cidadão")
            .WithOpenApi()
            .RequireAuthorization(Politicas.Cidadao);

        grupo.MapPost("/", async (CriarReporteRequest request, ClaimsPrincipal user, ReporteService service) =>
            {
                var erros = ReporteValidator.Validar(request);
                if (erros.Count > 0)
                {
                    return Results.ValidationProblem(erros);
                }

                var usuarioId = user.ObterUsuarioId();
                if (!await service.UsuarioExisteAsync(usuarioId))
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

                var reporte = await service.CriarAsync(usuarioId, categoria, request);
                return Results.Created($"/api/reportes/{reporte.Id}", ReporteMapper.ParaResponse(reporte));
            })
            .WithName("CriarReporte")
            .WithSummary("Registra um novo reporte de problema no espaço público")
            .Produces<ReporteResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
