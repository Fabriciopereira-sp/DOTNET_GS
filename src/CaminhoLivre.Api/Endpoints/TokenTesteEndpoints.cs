using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Endpoints;

// O login oficial fica na API Java. Este endpoint existe para testar a API .NET sozinha
// e só é publicado em desenvolvimento ou com Auth:HabilitarTokenDeTeste = true.
public static class TokenTesteEndpoints
{
    public static IEndpointRouteBuilder MapTokenTesteEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/dev/token", async (TokenTesteRequest request, AppDbContext db, TokenService tokens) =>
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["email"] = ["Informe o email de um usuário de teste."]
                    });
                }

                var email = request.Email.Trim().ToLowerInvariant();
                var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
                if (usuario is null)
                {
                    return Results.NotFound();
                }

                var (token, expiraEm) = tokens.Gerar(usuario);
                return Results.Ok(new TokenResponse(token, expiraEm,
                    new UsuarioTokenResponse(usuario.Id, usuario.Nome, usuario.Role)));
            })
            .WithTags("Token de teste")
            .WithName("GerarTokenDeTeste")
            .WithSummary("Gera um JWT para um usuário de teste (somente desenvolvimento)")
            .WithOpenApi()
            .AllowAnonymous()
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
