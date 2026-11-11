namespace CaminhoLivre.Api.Domain;

public class HistoricoStatus
{
    public long Id { get; set; }
    public long ReporteId { get; set; }
    public StatusReporte? StatusAnterior { get; set; }
    public StatusReporte StatusNovo { get; set; }
    public string? Observacao { get; set; }
    public long UsuarioId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public Reporte? Reporte { get; set; }
}
