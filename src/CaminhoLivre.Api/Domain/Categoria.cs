namespace CaminhoLivre.Api.Domain;

public class Categoria
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public long OrgaoId { get; set; }

    public Orgao? Orgao { get; set; }
}
