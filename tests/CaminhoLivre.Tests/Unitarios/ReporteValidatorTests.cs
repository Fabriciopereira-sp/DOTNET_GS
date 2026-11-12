using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Validation;

namespace CaminhoLivre.Tests.Unitarios;

public class ReporteValidatorTests
{
    private static CriarReporteRequest RequestValido() => new(
        Categoria: "CALCADA",
        Descricao: "Calçada quebrada em frente ao número 120",
        Latitude: -23.561414m,
        Longitude: -46.655881m,
        Endereco: "Av. Paulista, 120",
        Urgencia: "ALTA");

    [Fact]
    public void Validar_RequestCompleto_NaoRetornaErros()
    {
        Assert.Empty(ReporteValidator.Validar(RequestValido()));
    }

    [Fact]
    public void Validar_UrgenciaEmMinusculo_EAceita()
    {
        Assert.Empty(ReporteValidator.Validar(RequestValido() with { Urgencia = "media" }));
    }

    [Fact]
    public void Validar_SemCategoria_RetornaErroDeCategoria()
    {
        var erros = ReporteValidator.Validar(RequestValido() with { Categoria = " " });

        Assert.Contains("categoria", erros.Keys);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("curta")]
    public void Validar_DescricaoAusenteOuCurta_RetornaErro(string? descricao)
    {
        var erros = ReporteValidator.Validar(RequestValido() with { Descricao = descricao });

        Assert.Contains("descricao", erros.Keys);
    }

    [Fact]
    public void Validar_DescricaoMaiorQue500_RetornaErro()
    {
        var erros = ReporteValidator.Validar(RequestValido() with { Descricao = new string('a', 501) });

        Assert.Contains("descricao", erros.Keys);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validar_LatitudeForaDoIntervalo_RetornaErro(double latitude)
    {
        var erros = ReporteValidator.Validar(RequestValido() with { Latitude = (decimal)latitude });

        Assert.Contains("latitude", erros.Keys);
    }

    [Fact]
    public void Validar_SemLongitude_RetornaErro()
    {
        var erros = ReporteValidator.Validar(RequestValido() with { Longitude = null });

        Assert.Contains("longitude", erros.Keys);
    }

    [Theory]
    [InlineData("URGENTISSIMA")]
    [InlineData("1")]
    [InlineData(null)]
    public void Validar_UrgenciaInvalida_RetornaErro(string? urgencia)
    {
        var erros = ReporteValidator.Validar(RequestValido() with { Urgencia = urgencia });

        Assert.Contains("urgencia", erros.Keys);
    }

    [Theory]
    [InlineData(-1, 10, "page")]
    [InlineData(0, 0, "size")]
    [InlineData(0, 51, "size")]
    public void ValidarPaginacao_ValoresInvalidos_RetornaErro(int pagina, int tamanho, string campo)
    {
        Assert.Contains(campo, ReporteValidator.ValidarPaginacao(pagina, tamanho).Keys);
    }
}
