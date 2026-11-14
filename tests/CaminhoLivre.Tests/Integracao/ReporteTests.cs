using System.Net;
using System.Net.Http.Json;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Tests.Integracao;

[Collection(ColecaoIntegracao.Nome)]
public class ReporteTests(ApiFactory api)
{
    [Fact]
    public async Task Criar_ComDadosValidos_Retorna201ComProtocolo()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        var resposta = await cliente.CriarReporteAsync(Requisicoes.NovoReporte("CALCADA"));
        var reporte = await resposta.Content.ReadFromJsonAsync<ReporteResponse>();

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);
        Assert.StartsWith("VV-", reporte!.Protocolo);
        Assert.Equal("ABERTO", reporte.Status);
        Assert.Equal("Secretaria de Obras", reporte.Orgao);
        Assert.Equal("CALCADA", reporte.Categoria.Codigo);
        Assert.Equal(DateTimeKind.Utc, reporte.CriadoEm.Kind);
    }

    [Fact]
    public async Task Criar_ComDadosInvalidos_Retorna400ComOsCampos()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var invalido = new CriarReporteRequest("", "curta", 200m, null, null, "URGENTE");

        var resposta = await cliente.CriarReporteAsync(invalido);
        var corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("descricao", corpo);
        Assert.Contains("latitude", corpo);
    }

    [Fact]
    public async Task Criar_ComCategoriaInexistente_Retorna400()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        var resposta = await cliente.CriarReporteAsync(Requisicoes.NovoReporte("NAO_EXISTE"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task ListarMeus_MostraSoOsReportesDoUsuario()
    {
        var maria = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var joao = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        await maria.CriarReporteComSucessoAsync();
        await maria.CriarReporteComSucessoAsync();
        await joao.CriarReporteComSucessoAsync();

        var pagina = await maria.GetFromJsonAsync<PaginaResponse<ReporteResponse>>("/api/reportes/meus?page=1&size=10");

        Assert.Equal(2, pagina!.Total);
    }

    [Fact]
    public async Task Detalhe_DeReporteDeOutroCidadao_Retorna404()
    {
        var dono = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var outro = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await dono.CriarReporteComSucessoAsync();

        var resposta = await outro.GetAsync($"/api/reportes/{reporte.Id}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Detalhe_DoProprioReporte_TrazOHistorico()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cliente.CriarReporteComSucessoAsync();

        var detalhe = await cliente.GetFromJsonAsync<ReporteDetalheResponse>($"/api/reportes/{reporte.Id}");

        Assert.Single(detalhe!.Historico);
        Assert.Equal("ABERTO", detalhe.Historico.First().StatusNovo);
    }

    [Fact]
    public async Task Cancelar_DepoisNaoPodeEditar()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cliente.CriarReporteComSucessoAsync();

        var cancelamento = await cliente.DeleteAsync($"/api/reportes/{reporte.Id}");
        var edicao = await cliente.PutAsJsonAsync($"/api/reportes/{reporte.Id}",
            new AtualizarReporteRequest("Descrição nova depois de cancelado", null, null));

        Assert.Equal(HttpStatusCode.NoContent, cancelamento.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, edicao.StatusCode);
    }

    [Fact]
    public async Task Foto_PngValida_Retorna200ComUrl()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cliente.CriarReporteComSucessoAsync();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };

        var resposta = await cliente.PostAsync($"/api/reportes/{reporte.Id}/foto", Formulario(png, "foto.png"));
        var atualizado = await resposta.Content.ReadFromJsonAsync<ReporteResponse>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.StartsWith("/uploads/", atualizado!.FotoUrl);
    }

    [Fact]
    public async Task Foto_ExecutavelRenomeado_Retorna400()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cliente.CriarReporteComSucessoAsync();
        var executavel = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };

        var resposta = await cliente.PostAsync($"/api/reportes/{reporte.Id}/foto", Formulario(executavel, "foto.jpg"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private static MultipartFormDataContent Formulario(byte[] conteudo, string nome) =>
        new() { { new ByteArrayContent(conteudo), "foto", nome } };
}
