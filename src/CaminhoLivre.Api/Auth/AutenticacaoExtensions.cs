using System.Security.Cryptography;
using System.Text;
using CaminhoLivre.Api.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CaminhoLivre.Api.Auth;

public static class AutenticacaoExtensions
{
    // O token é emitido pela API Java (login). Aqui só validamos, com a mesma chave (JWT_SECRET),
    // o mesmo emissor e o mesmo público definidos no contrato do grupo.
    public static WebApplicationBuilder AddAutenticacaoJwt(this WebApplicationBuilder builder)
    {
        var opcoes = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var chave = ObterChave(builder);

        builder.Services.AddSingleton(new ConfiguracaoJwt(opcoes, chave));

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opcoes.Issuer,
                    ValidateAudience = true,
                    ValidAudience = opcoes.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = chave,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Politicas.Cidadao, p => p.RequireRole(Papeis.Cidadao))
            .AddPolicy(Politicas.Gestor, p => p.RequireRole(Papeis.Gestor));

        return builder;
    }

    private static SymmetricSecurityKey ObterChave(WebApplicationBuilder builder)
    {
        var segredo = builder.Configuration["JWT_SECRET"];

        if (!string.IsNullOrWhiteSpace(segredo))
        {
            if (Encoding.UTF8.GetByteCount(segredo) < 32)
            {
                throw new InvalidOperationException("JWT_SECRET precisa ter pelo menos 32 caracteres.");
            }

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo));
        }

        // Sem JWT_SECRET em desenvolvimento, gera uma chave temporária para não deixar segredo no repositório.
        // Nesse caso só funcionam os tokens gerados pelo endpoint de teste desta própria API.
        if (builder.Environment.IsDevelopment())
        {
            return new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64));
        }

        throw new InvalidOperationException("Variável de ambiente JWT_SECRET não configurada.");
    }
}
