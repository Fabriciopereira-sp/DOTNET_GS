using CaminhoLivre.Api.Domain;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CaminhoLivre.Api.Auth;

// Gera tokens no mesmo formato da API Java. Usado apenas pelo endpoint de token de teste.
public class TokenService(ConfiguracaoJwt configuracao)
{
    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
    {
        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(configuracao.Opcoes.ExpiracaoMinutos);

        var descritor = new SecurityTokenDescriptor
        {
            Issuer = configuracao.Opcoes.Issuer,
            Audience = configuracao.Opcoes.Audience,
            IssuedAt = agora,
            NotBefore = agora,
            Expires = expiraEm,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = usuario.Id.ToString(),
                ["name"] = usuario.Nome,
                ["role"] = usuario.Role
            },
            SigningCredentials = new SigningCredentials(configuracao.Chave, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descritor);
        return (token, expiraEm);
    }
}
