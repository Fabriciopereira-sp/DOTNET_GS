using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Endpoints;

public static class CatalogoEndpoints
{
    public static IEndpointRouteBuilder MapCatalogoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api")
            .WithTags("Catálogo")
            .WithOpenApi();

        grupo.MapGet("/orgaos", async (AppDbContext db) =>
            {
                var orgaos = await db.Orgaos.AsNoTracking().OrderBy(o => o.Id).ToListAsync();
                return Results.Ok(orgaos.Select(o => new OrgaoResponse(o.Id, o.Nome, o.Area.ToString())));
            })
            .WithName("ListarOrgaos")
            .WithSummary("Lista os órgãos públicos que atendem os reportes")
            .Produces<IEnumerable<OrgaoResponse>>(StatusCodes.Status200OK);

        grupo.MapGet("/categorias", async (AppDbContext db) =>
            {
                var categorias = await db.Categorias.AsNoTracking()
                    .Include(c => c.Orgao)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                return Results.Ok(categorias.Select(c =>
                    new CategoriaResponse(c.Id, c.Codigo, c.Nome, c.OrgaoId, c.Orgao!.Nome)));
            })
            .WithName("ListarCategorias")
            .WithSummary("Lista as categorias de problema e o órgão responsável por cada uma")
            .Produces<IEnumerable<CategoriaResponse>>(StatusCodes.Status200OK);

        return app;
    }
}
