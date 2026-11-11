namespace CaminhoLivre.Api.Domain;

// Guarda a resposta de uma requisição para devolver a mesma resposta se ela for repetida.
public class RegistroIdempotencia
{
    public string Chave { get; set; } = string.Empty;
    public long UsuarioId { get; set; }
    public string HashCorpo { get; set; } = string.Empty;
    public int StatusHttp { get; set; }
    public string? RespostaJson { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
