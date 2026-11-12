using System.Net;
using System.Net.Http.Json;
using System.Text;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CaminhoLivre.Tests.Integracao;

[Collection(ColecaoIntegracao.Nome)]
public class AutenticacaoTests(ApiFactory api)
{
    [Fact]
    public async Task SemToken_Retorna401()
    {
        var resposta = await api.CreateClient().GetAsync("/api/categorias");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task ComTokenValido_Retorna200()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        var categorias = await cliente.GetFromJsonAsync<List<CategoriaResponse>>("/api/categorias");

        Assert.NotNull(categorias);
        Assert.Contains(categorias, c => c.Codigo == "CALCADA");
    }

    [Fact]
    public async Task TokenAssinadoComOutraChave_Retorna401()
    {
        var chaveFalsa = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("uma-chave-diferente-da-chave-da-api-123456789"));
        var tokenFalso = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "voz-e-via-java",
            Audience = "voz-e-via",
            Expires = DateTime.UtcNow.AddMinutes(10),
            Claims = new Dictionary<string, object> { ["sub"] = "1", ["role"] = Papeis.Cidadao },
            SigningCredentials = new SigningCredentials(chaveFalsa, SecurityAlgorithms.HmacSha256)
        });

        var resposta = await api.ClienteComToken(tokenFalso).GetAsync("/api/categorias");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task CidadaoNoPainelDoGestor_Retorna403()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        var resposta = await cliente.GetAsync("/api/gestor/reportes");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task GestorCriandoReporte_Retorna403()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Gestor, orgaoId: 1);

        var resposta = await cliente.CriarReporteAsync(Requisicoes.NovoReporte());

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
