using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Tests.Integracao;

// Formato combinado com o app mobile: foto enviada antes do reporte, JSON em camelCase,
// datas em UTC com "Z" e erros com "detail" em português.
[Collection(ColecaoIntegracao.Nome)]
public class ContratoMobileTests(ApiFactory api)
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];

    private static MultipartFormDataContent Arquivo(byte[] conteudo, string nome) =>
        new() { { new ByteArrayContent(conteudo), "arquivo", nome } };

    [Fact]
    public async Task EnviarFotoAntes_DepoisCriarReporteComAFotoUrl()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        var envio = await cliente.PostAsync("/api/fotos", Arquivo(Png, "foto.png"));
        var foto = await envio.Content.ReadFromJsonAsync<FotoResponse>();
        var criacao = await cliente.CriarReporteAsync(Requisicoes.NovoReporte() with { FotoUrl = foto!.FotoUrl });
        var reporte = await criacao.Content.ReadFromJsonAsync<ReporteResponse>();

        Assert.Equal(HttpStatusCode.Created, envio.StatusCode);
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        Assert.Equal(foto.FotoUrl, reporte!.FotoUrl);
    }

    [Fact]
    public async Task CriarReporte_ComFotoUrlQueNaoExiste_Retorna400()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var inexistente = "/uploads/0123456789abcdef0123456789abcdef.png";

        var resposta = await cliente.CriarReporteAsync(Requisicoes.NovoReporte() with { FotoUrl = inexistente });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task EditarReporte_SubstituiTudoERemoveAFoto()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var envio = await cliente.PostAsync("/api/fotos", Arquivo(Png, "foto.png"));
        var foto = await envio.Content.ReadFromJsonAsync<FotoResponse>();
        var criado = await (await cliente.CriarReporteAsync(Requisicoes.NovoReporte() with { FotoUrl = foto!.FotoUrl }))
            .Content.ReadFromJsonAsync<ReporteResponse>();

        var edicao = await cliente.PutAsJsonAsync($"/api/reportes/{criado!.Id}",
            new AtualizarReporteRequest("Descrição corrigida pelo cidadão", null, null));
        var detalhe = await edicao.Content.ReadFromJsonAsync<ReporteDetalheResponse>();

        Assert.Equal(HttpStatusCode.OK, edicao.StatusCode);
        Assert.Equal("Descrição corrigida pelo cidadão", detalhe!.Descricao);
        Assert.Null(detalhe.Endereco);
        Assert.Null(detalhe.FotoUrl);
    }

    [Fact]
    public async Task Detalhe_TemCategoriaComoObjetoHistoricoEDataComZ()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        var reporte = await cliente.CriarReporteComSucessoAsync("RAMPA");

        var json = await cliente.GetStringAsync($"/api/reportes/{reporte.Id}");
        using var documento = JsonDocument.Parse(json);
        var raiz = documento.RootElement;

        Assert.Equal("RAMPA", raiz.GetProperty("categoria").GetProperty("codigo").GetString());
        Assert.Equal("ABERTO", raiz.GetProperty("status").GetString());
        Assert.Equal(1, raiz.GetProperty("historico").GetArrayLength());
        Assert.EndsWith("Z", raiz.GetProperty("criadoEm").GetString());
    }

    [Fact]
    public async Task ListaPaginada_ComecaNaPagina1()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);
        await cliente.CriarReporteComSucessoAsync();

        var pagina = await cliente.GetFromJsonAsync<PaginaResponse<ReporteResponse>>("/api/reportes/meus");
        var paginaZero = await cliente.GetAsync("/api/reportes/meus?page=0");

        Assert.Equal(1, pagina!.Pagina);
        Assert.Equal(1, pagina.Total);
        Assert.Equal(HttpStatusCode.BadRequest, paginaZero.StatusCode);
    }

    [Fact]
    public async Task NaoEncontrado_TrazDetailEmPortugues()
    {
        var cliente = await api.ClienteAutenticadoAsync(Papeis.Cidadao);

        var resposta = await cliente.GetAsync("/api/reportes/999999");
        using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("Reporte não encontrado.", documento.RootElement.GetProperty("detail").GetString());
    }
}
