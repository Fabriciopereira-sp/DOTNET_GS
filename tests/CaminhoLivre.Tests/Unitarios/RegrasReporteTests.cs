using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Tests.Unitarios;

public class RegrasReporteTests
{
    private static readonly DateTime Agora = new(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void GerarProtocolo_UsaAnoEIdComSeisDigitos()
    {
        Assert.Equal("VV-2026-000123", RegrasReporte.GerarProtocolo(123, 2026));
    }

    [Theory]
    [InlineData(Urgencia.ALTA, 100)]
    [InlineData(Urgencia.MEDIA, 50)]
    [InlineData(Urgencia.BAIXA, 10)]
    public void CalcularPrioridade_ReporteNovo_UsaSoOPesoDaUrgencia(Urgencia urgencia, int esperado)
    {
        Assert.Equal(esperado, RegrasReporte.CalcularPrioridade(urgencia, Agora, Agora));
    }

    [Fact]
    public void CalcularPrioridade_SomaDoisPontosPorDiaEmAberto()
    {
        var criadoHaCincoDias = Agora.AddDays(-5);

        Assert.Equal(60, RegrasReporte.CalcularPrioridade(Urgencia.MEDIA, criadoHaCincoDias, Agora));
    }

    [Fact]
    public void CalcularPrioridade_LimitaOsDiasEmTrinta()
    {
        var criadoHaNoventaDias = Agora.AddDays(-90);

        Assert.Equal(70, RegrasReporte.CalcularPrioridade(Urgencia.BAIXA, criadoHaNoventaDias, Agora));
    }

    [Fact]
    public void CalcularPrioridade_BaixaAntigaPodePassarMediaNova()
    {
        var baixaAntiga = RegrasReporte.CalcularPrioridade(Urgencia.BAIXA, Agora.AddDays(-25), Agora);
        var mediaNova = RegrasReporte.CalcularPrioridade(Urgencia.MEDIA, Agora, Agora);

        Assert.True(baixaAntiga > mediaNova);
    }

    [Theory]
    [InlineData(StatusReporte.ABERTO, StatusReporte.EM_ANALISE)]
    [InlineData(StatusReporte.ABERTO, StatusReporte.CANCELADO)]
    [InlineData(StatusReporte.EM_ANALISE, StatusReporte.EM_EXECUCAO)]
    [InlineData(StatusReporte.EM_EXECUCAO, StatusReporte.RESOLVIDO)]
    public void PodeMudarStatus_TransicoesDoFluxo_SaoPermitidas(StatusReporte atual, StatusReporte novo)
    {
        Assert.True(RegrasReporte.PodeMudarStatus(atual, novo));
    }

    [Theory]
    [InlineData(StatusReporte.ABERTO, StatusReporte.RESOLVIDO)]
    [InlineData(StatusReporte.ABERTO, StatusReporte.EM_EXECUCAO)]
    [InlineData(StatusReporte.EM_ANALISE, StatusReporte.ABERTO)]
    [InlineData(StatusReporte.EM_ANALISE, StatusReporte.CANCELADO)]
    [InlineData(StatusReporte.EM_EXECUCAO, StatusReporte.EM_ANALISE)]
    [InlineData(StatusReporte.RESOLVIDO, StatusReporte.ABERTO)]
    [InlineData(StatusReporte.CANCELADO, StatusReporte.ABERTO)]
    public void PodeMudarStatus_PularOuVoltarEtapa_NaoEPermitido(StatusReporte atual, StatusReporte novo)
    {
        Assert.False(RegrasReporte.PodeMudarStatus(atual, novo));
    }

    [Theory]
    [InlineData(StatusReporte.ABERTO, true)]
    [InlineData(StatusReporte.EM_ANALISE, false)]
    [InlineData(StatusReporte.RESOLVIDO, false)]
    [InlineData(StatusReporte.CANCELADO, false)]
    public void PodeSerEditado_SoEnquantoAberto(StatusReporte status, bool esperado)
    {
        Assert.Equal(esperado, RegrasReporte.PodeSerEditado(status));
    }
}
