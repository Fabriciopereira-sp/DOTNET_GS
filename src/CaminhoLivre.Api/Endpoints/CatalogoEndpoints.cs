using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Services;
using CaminhoLivre.Api.Validation;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Endpoints;

public static class CatalogoEndpoints
{
    public static IEndpointRouteBuilder MapCatalogoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api")
            .WithTags("Catálogo")
            .WithOpenApi()
            .RequireAuthorization();

        grupo.MapGet("/orgaos", async (AppDbContext db) =>
            {
                var orgaos = await db.Orgaos.AsNoTracking().OrderBy(o => o.Id).ToListAsync();
                return Results.Ok(orgaos.Select(o => new OrgaoResponse(o.Id, o.Nome, o.Area.ToString())));
            })
            .WithName("ListarOrgaos")
            .WithSummary("Lista os órgãos públicos que atendem os reportes")
            .Produces<IEnumerable<OrgaoResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/categorias", async (CategoriaService service) => Results.Ok(await service.ListarAsync()))
            .WithName("ListarCategorias")
            .WithSummary("Lista as categorias de problema e o órgão responsável por cada uma")
            .Produces<IEnumerable<CategoriaResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        grupo.MapGet("/categorias/{id:long}", async (long id, CategoriaService service) =>
                await service.BuscarAsync(id) is { } categoria
                    ? Results.Ok(CategoriaService.ParaResponse(categoria))
                    : Results.NotFound())
            .WithName("BuscarCategoria")
            .WithSummary("Busca uma categoria pelo ID")
            .Produces<CategoriaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapPost("/categorias", async (CategoriaRequest request, CategoriaService service) =>
            {
                var erro = await ValidarAsync(request, service, null);
                if (erro is not null)
                {
                    return erro;
                }

                var categoria = await service.CriarAsync(request);
                return Results.Created($"/api/categorias/{categoria.Id}", CategoriaService.ParaResponse(categoria));
            })
            .RequireAuthorization(Politicas.Gestor)
            .WithName("CriarCategoria")
            .WithSummary("Cria uma categoria de problema (somente gestor)")
            .Produces<CategoriaResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPut("/categorias/{id:long}", async (long id, CategoriaRequest request, CategoriaService service) =>
            {
                var categoria = await service.BuscarAsync(id);
                if (categoria is null)
                {
                    return Results.NotFound();
                }

                var erro = await ValidarAsync(request, service, id);
                if (erro is not null)
                {
                    return erro;
                }

                await service.AtualizarAsync(categoria, request);
                return Results.Ok(CategoriaService.ParaResponse(categoria));
            })
            .RequireAuthorization(Politicas.Gestor)
            .WithName("AtualizarCategoria")
            .WithSummary("Atualiza uma categoria (somente gestor)")
            .Produces<CategoriaResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/categorias/{id:long}", async (long id, CategoriaService service) =>
            {
                var categoria = await service.BuscarAsync(id);
                if (categoria is null)
                {
                    return Results.NotFound();
                }

                if (await service.PossuiReportesAsync(id))
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Categoria em uso",
                        detail: "Existem reportes nesta categoria. Ela não pode ser excluída.");
                }

                await service.ExcluirAsync(categoria);
                return Results.NoContent();
            })
            .RequireAuthorization(Politicas.Gestor)
            .WithName("ExcluirCategoria")
            .WithSummary("Exclui uma categoria sem reportes vinculados (somente gestor)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult?> ValidarAsync(CategoriaRequest request, CategoriaService service, long? idAtual)
    {
        var erros = CategoriaValidator.Validar(request);
        if (erros.Count > 0)
        {
            return Results.ValidationProblem(erros);
        }

        if (!await service.OrgaoExisteAsync(request.OrgaoId!.Value))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["orgaoId"] = ["Órgão não encontrado. Consulte GET /api/orgaos."]
            });
        }

        if (await service.CodigoEmUsoAsync(CategoriaService.NormalizarCodigo(request.Codigo), idAtual))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Código já cadastrado",
                detail: "Já existe uma categoria com esse código.");
        }

        return null;
    }
}
