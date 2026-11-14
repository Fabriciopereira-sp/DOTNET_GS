using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Infra;
using CaminhoLivre.Api.Validation;

namespace CaminhoLivre.Api.Endpoints;

public static class FotoEndpoints
{
    public static IEndpointRouteBuilder MapFotoEndpoints(this IEndpointRouteBuilder app)
    {
        // Fluxo do app: envia a foto primeiro, recebe a fotoUrl e depois cria ou edita o reporte com ela.
        app.MapPost("/api/fotos", async (IFormFile? arquivo, IArmazenamentoDeFotos armazenamento, CancellationToken cancellationToken) =>
            {
                var (extensao, erro) = await FotoValidator.ValidarAsync(arquivo, cancellationToken, campo: "arquivo");
                if (erro is not null)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["arquivo"] = [erro] });
                }

                await using var conteudo = arquivo!.OpenReadStream();
                var fotoUrl = await armazenamento.SalvarAsync(conteudo, extensao!, cancellationToken);

                return Results.Created(fotoUrl, new FotoResponse(fotoUrl));
            })
            .RequireAuthorization(Politicas.Cidadao)
            .DisableAntiforgery()
            .WithTags("Fotos")
            .WithName("EnviarFoto")
            .WithSummary("Envia uma foto (JPEG ou PNG, até 5 MB, campo 'arquivo') e devolve a fotoUrl para usar no reporte")
            .WithOpenApi()
            .Produces<FotoResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
