namespace CaminhoLivre.Api.Contracts;

public record TokenTesteRequest(string? Email);

public record UsuarioTokenResponse(long Id, string Nome, string Role);

public record TokenResponse(string Token, DateTime ExpiraEm, UsuarioTokenResponse Usuario);
