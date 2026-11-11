namespace CaminhoLivre.Api.Contracts;

public record CriarReporteRequest(
    string? Categoria,
    string? Descricao,
    decimal? Latitude,
    decimal? Longitude,
    string? Endereco,
    string? Urgencia);

public record ReporteResponse(
    long Id,
    string Protocolo,
    string Categoria,
    string CategoriaNome,
    string Orgao,
    string Descricao,
    decimal Latitude,
    decimal Longitude,
    string? Endereco,
    string? FotoUrl,
    string Urgencia,
    string Status,
    int Prioridade,
    DateTime CriadoEm,
    DateTime AtualizadoEm);
