namespace CaminhoLivre.Api.Domain;

public static class RegrasReporte
{
    private static readonly Dictionary<StatusReporte, StatusReporte[]> TransicoesPermitidas = new()
    {
        [StatusReporte.ABERTO] = [StatusReporte.EM_ANALISE, StatusReporte.CANCELADO],
        [StatusReporte.EM_ANALISE] = [StatusReporte.EM_EXECUCAO],
        [StatusReporte.EM_EXECUCAO] = [StatusReporte.RESOLVIDO],
        [StatusReporte.RESOLVIDO] = [],
        [StatusReporte.CANCELADO] = []
    };

    public static string GerarProtocolo(long id, int ano) => $"CL-{ano}-{id:D6}";

    // Prioridade da fila do gestor: peso da urgência + 2 pontos por dia em aberto (até 30 dias).
    public static int CalcularPrioridade(Urgencia urgencia, DateTime criadoEm, DateTime agora)
    {
        var peso = urgencia switch
        {
            Urgencia.ALTA => 100,
            Urgencia.MEDIA => 50,
            _ => 10
        };

        var dias = Math.Clamp((int)(agora - criadoEm).TotalDays, 0, 30);
        return peso + dias * 2;
    }

    // Fluxo válido: ABERTO -> EM_ANALISE -> EM_EXECUCAO -> RESOLVIDO, ou ABERTO -> CANCELADO.
    public static bool PodeMudarStatus(StatusReporte atual, StatusReporte novo) =>
        TransicoesPermitidas[atual].Contains(novo);

    public static bool PodeSerEditado(StatusReporte status) => status == StatusReporte.ABERTO;
}
