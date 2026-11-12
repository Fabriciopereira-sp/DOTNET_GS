using System.Net;

namespace CaminhoLivre.Tests.Integracao;

[Collection(ColecaoIntegracao.Nome)]
public class HealthCheckTests(ApiFactory api)
{
    [Fact]
    public async Task Health_SemLogin_Retorna200ComAsVerificacoes()
    {
        var resposta = await api.CreateClient().GetAsync("/health");
        var corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("banco-de-dados", corpo);
        Assert.Contains("armazenamento-de-fotos", corpo);
        Assert.Contains("api-java", corpo);
    }

    [Fact]
    public async Task HealthLive_Retorna200()
    {
        var resposta = await api.CreateClient().GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}
