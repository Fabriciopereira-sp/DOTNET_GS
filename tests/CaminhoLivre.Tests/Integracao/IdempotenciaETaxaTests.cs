using System.Net;
using System.Net.Http.Json;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Tests.Integracao;

[Collection(ColecaoIntegracao.Nome)]
public class IdempotenciaETaxaTests(ApiFactory api)
{
    [Fact]
    public async Task SemIdempotencyKey_Retorna400()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        var resposta = await cliente.PostAsJsonAsync("/api/reportes", Requisicoes.NovoReporte());

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task MesmaChaveEMesmoCorpo_DevolveOMesmoReporteSemDuplicar()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var chave = Guid.NewGuid().ToString();
        var request = Requisicoes.NovoReporte();

        var primeira = await cliente.CriarReporteAsync(request, chave);
        var segunda = await cliente.CriarReporteAsync(request, chave);

        var reportePrimeira = await primeira.Content.ReadFromJsonAsync<ReporteResponse>();
        var reporteSegunda = await segunda.Content.ReadFromJsonAsync<ReporteResponse>();
        var meus = await cliente.GetFromJsonAsync<PaginaResponse<ReporteResponse>>("/api/reportes/meus");

        Assert.Equal(HttpStatusCode.Created, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
        Assert.Equal(reportePrimeira!.Protocolo, reporteSegunda!.Protocolo);
        Assert.True(segunda.Headers.Contains("Idempotent-Replayed"));
        Assert.Equal(1, meus!.TotalItens);
    }

    [Fact]
    public async Task MesmaChaveComCorpoDiferente_Retorna422()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var chave = Guid.NewGuid().ToString();

        await cliente.CriarReporteAsync(Requisicoes.NovoReporte(descricao: "Primeiro conteúdo do reporte"), chave);
        var resposta = await cliente.CriarReporteAsync(Requisicoes.NovoReporte(descricao: "Conteúdo diferente do primeiro"), chave);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task MaisDeCincoReportesPorMinuto_Retorna429ComRetryAfter()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        for (var i = 0; i < 5; i++)
        {
            var permitida = await cliente.CriarReporteAsync(Requisicoes.NovoReporte());
            Assert.Equal(HttpStatusCode.Created, permitida.StatusCode);
        }

        var bloqueada = await cliente.CriarReporteAsync(Requisicoes.NovoReporte());

        Assert.Equal(HttpStatusCode.TooManyRequests, bloqueada.StatusCode);
        Assert.NotNull(bloqueada.Headers.RetryAfter);
    }
}
