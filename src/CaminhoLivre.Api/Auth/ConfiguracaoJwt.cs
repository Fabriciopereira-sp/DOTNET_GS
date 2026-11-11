using Microsoft.IdentityModel.Tokens;

namespace CaminhoLivre.Api.Auth;

public record ConfiguracaoJwt(JwtOptions Opcoes, SymmetricSecurityKey Chave);
