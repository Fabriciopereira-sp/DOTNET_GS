using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Validation;

namespace CaminhoLivre.Tests.Unitarios;

public class CategoriaValidatorTests
{
    [Fact]
    public void Validar_CategoriaValida_NaoRetornaErros()
    {
        Assert.Empty(CategoriaValidator.Validar(new CategoriaRequest("PISO_TATIL", "Piso tátil danificado", 3)));
    }

    [Fact]
    public void Validar_CodigoEmMinusculo_EAceitoPoisEhNormalizado()
    {
        Assert.Empty(CategoriaValidator.Validar(new CategoriaRequest("arvore_caida", "Árvore caída", 1)));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("COM ESPACO")]
    [InlineData("COM-HIFEN")]
    [InlineData("1NUMERO")]
    [InlineData(null)]
    public void Validar_CodigoForaDoPadrao_RetornaErro(string? codigo)
    {
        var erros = CategoriaValidator.Validar(new CategoriaRequest(codigo, "Nome válido", 1));

        Assert.Contains("codigo", erros.Keys);
    }

    [Fact]
    public void Validar_SemOrgao_RetornaErro()
    {
        var erros = CategoriaValidator.Validar(new CategoriaRequest("BURACO", "Buraco na via", null));

        Assert.Contains("orgaoId", erros.Keys);
    }
}
