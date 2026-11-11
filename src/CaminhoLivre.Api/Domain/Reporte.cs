namespace CaminhoLivre.Api.Domain;

public class Reporte
{
    public long Id { get; set; }
    public string Protocolo { get; set; } = string.Empty;
    public long UsuarioId { get; set; }
    public long CategoriaId { get; set; }
    public long OrgaoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Endereco { get; set; }
    public string? FotoUrl { get; set; }
    public Urgencia Urgencia { get; set; }
    public StatusReporte Status { get; set; } = StatusReporte.ABERTO;
    public int Prioridade { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
    public Categoria? Categoria { get; set; }
    public Orgao? Orgao { get; set; }
    public List<HistoricoStatus> Historico { get; set; } = [];
}
