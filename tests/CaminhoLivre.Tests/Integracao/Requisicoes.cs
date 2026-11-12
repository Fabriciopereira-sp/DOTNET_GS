using System.Net.Http.Json;
using CaminhoLivre.Api.Contracts;

namespace CaminhoLivre.Tests.Integracao;

public static class Requisicoes
{
    public static CriarReporteRequest NovoReporte(string categoria = "BURACO", string descricao = "Buraco grande no meio da faixa de pedestres") =>
        new(categoria, descricao, -23.561414m, -46.655881m, "Av. Paulista, 1000", "ALTA");

    public static async Task<HttpResponseMessage> CriarReporteAsync(
        this HttpClient cliente, CriarReporteRequest request, string? chave = null)
    {
        var mensagem = new HttpRequestMessage(HttpMethod.Post, "/api/reportes")
        {
            Content = JsonContent.Create(request)
        };
        mensagem.Headers.Add("Idempotency-Key", chave ?? Guid.NewGuid().ToString());
        return await cliente.SendAsync(mensagem);
    }

    public static async Task<ReporteResponse> CriarReporteComSucessoAsync(this HttpClient cliente, string categoria = "BURACO")
    {
        var resposta = await cliente.CriarReporteAsync(NovoReporte(categoria));
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ReporteResponse>())!;
    }
}
