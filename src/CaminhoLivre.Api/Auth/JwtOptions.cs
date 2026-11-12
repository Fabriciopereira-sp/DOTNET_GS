namespace CaminhoLivre.Api.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "voz-e-via-java";
    public string Audience { get; set; } = "voz-e-via";
    public int ExpiracaoMinutos { get; set; } = 60;
}
