using System.Security.Claims;

namespace CaminhoLivre.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    // O usuário sempre vem do token (claim "sub"), nunca do corpo da requisição.
    public static long ObterUsuarioId(this ClaimsPrincipal usuario)
    {
        var sub = usuario.FindFirstValue("sub");
        return long.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("Token sem identificação do usuário.");
    }
}
