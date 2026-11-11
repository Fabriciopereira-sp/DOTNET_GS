namespace CaminhoLivre.Api.Contracts;

public record OrgaoResponse(long Id, string Nome, string Area);

public record CategoriaResponse(long Id, string Codigo, string Nome, long OrgaoId, string Orgao);

public record CategoriaRequest(string? Codigo, string? Nome, long? OrgaoId);
