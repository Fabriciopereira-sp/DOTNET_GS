namespace CaminhoLivre.Api.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "caminho-livre-java";
    public string Audience { get; set; } = "caminho-livre";
    public int ExpiracaoMinutos { get; set; } = 60;
}
