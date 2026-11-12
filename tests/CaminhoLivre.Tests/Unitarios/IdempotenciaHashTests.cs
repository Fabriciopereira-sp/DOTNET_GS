using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Services;

namespace CaminhoLivre.Tests.Unitarios;

public class IdempotenciaHashTests
{
    private static CriarReporteRequest Request(string descricao) =>
        new("BURACO", descricao, -23.5m, -46.6m, null, "ALTA");

    [Fact]
    public void CalcularHash_MesmoConteudo_GeraMesmoHash()
    {
        var primeiro = IdempotenciaService.CalcularHash(Request("Buraco grande na rua"));
        var segundo = IdempotenciaService.CalcularHash(Request("Buraco grande na rua"));

        Assert.Equal(primeiro, segundo);
    }

    [Fact]
    public void CalcularHash_ConteudoDiferente_GeraHashDiferente()
    {
        var primeiro = IdempotenciaService.CalcularHash(Request("Buraco grande na rua"));
        var segundo = IdempotenciaService.CalcularHash(Request("Buraco pequeno na rua"));

        Assert.NotEqual(primeiro, segundo);
    }
}
