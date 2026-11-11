namespace CaminhoLivre.Api.Domain;

// Tabela pertence à API Java (cadastro e login). A API .NET apenas lê.
public class Usuario
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = Papeis.Cidadao;
    public long? OrgaoId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public Orgao? Orgao { get; set; }
}
