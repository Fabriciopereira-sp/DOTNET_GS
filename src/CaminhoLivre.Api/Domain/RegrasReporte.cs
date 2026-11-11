namespace CaminhoLivre.Api.Domain;

public static class RegrasReporte
{
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
}
