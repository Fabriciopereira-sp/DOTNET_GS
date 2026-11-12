using System.Net;
using System.Net.Http.Json;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Tests.Integracao;

[Collection(ColecaoIntegracao.Nome)]
public class GestorTests(ApiFactory api)
{
    [Fact]
    public async Task Fila_MostraReporteDoOrgaoDoGestor()
    {
        var cidadao = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cidadao.CriarReporteComSucessoAsync("BURACO");
        var gestor = await api.ClienteAutenticadoAsync(Papeis.Gestor, await api.ObterOrgaoIdAsync(AreaOrgao.OBRAS));

        var fila = await gestor.GetFromJsonAsync<PaginaResponse<ReporteResponse>>("/api/gestor/reportes?size=50");

        Assert.Contains(fila!.Itens, r => r.Id == reporte.Id);
    }

    [Fact]
    public async Task MudarStatus_SeguindoOFluxo_Retorna200EGravaHistorico()
    {
        var cidadao = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cidadao.CriarReporteComSucessoAsync("BURACO");
        var gestor = await api.ClienteAutenticadoAsync(Papeis.Gestor, await api.ObterOrgaoIdAsync(AreaOrgao.OBRAS));

        var resposta = await gestor.PatchAsJsonAsync($"/api/gestor/reportes/{reporte.Id}/status",
            new MudarStatusRequest("EM_ANALISE", "Equipe vai avaliar no local"));
        var detalhe = await resposta.Content.ReadFromJsonAsync<ReporteDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("EM_ANALISE", detalhe!.Reporte.Status);
        Assert.Equal(2, detalhe.Historico.Count());
    }

    [Fact]
    public async Task MudarStatus_PulandoEtapa_Retorna409()
    {
        var cidadao = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cidadao.CriarReporteComSucessoAsync("BURACO");
        var gestor = await api.ClienteAutenticadoAsync(Papeis.Gestor, await api.ObterOrgaoIdAsync(AreaOrgao.OBRAS));

        var resposta = await gestor.PatchAsJsonAsync($"/api/gestor/reportes/{reporte.Id}/status",
            new MudarStatusRequest("RESOLVIDO", null));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task MudarStatus_DeReporteDeOutroOrgao_Retorna404()
    {
        var cidadao = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cidadao.CriarReporteComSucessoAsync("BURACO");
        var gestorIluminacao = await api.ClienteAutenticadoAsync(Papeis.Gestor, await api.ObterOrgaoIdAsync(AreaOrgao.ILUMINACAO));

        var resposta = await gestorIluminacao.PatchAsJsonAsync($"/api/gestor/reportes/{reporte.Id}/status",
            new MudarStatusRequest("EM_ANALISE", null));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task CrudDeCategoria_CriarAtualizarExcluir()
    {
        var orgaoObras = await api.ObterOrgaoIdAsync(AreaOrgao.OBRAS);
        var gestor = await api.ClienteAutenticadoAsync(Papeis.Gestor, orgaoObras);
        var codigo = $"TESTE_{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        codigo = new string(codigo.Where(c => char.IsLetter(c) || c == '_').ToArray());

        var criacao = await gestor.PostAsJsonAsync("/api/categorias", new CategoriaRequest(codigo, "Categoria de teste", orgaoObras));
        var criada = await criacao.Content.ReadFromJsonAsync<CategoriaResponse>();

        var atualizacao = await gestor.PutAsJsonAsync($"/api/categorias/{criada!.Id}",
            new CategoriaRequest(codigo, "Categoria de teste renomeada", orgaoObras));
        var exclusao = await gestor.DeleteAsync($"/api/categorias/{criada.Id}");
        var busca = await gestor.GetAsync($"/api/categorias/{criada.Id}");

        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        Assert.Equal(HttpStatusCode.OK, atualizacao.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, busca.StatusCode);
    }
}
