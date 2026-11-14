namespace CaminhoLivre.Api.Contracts;

public record CriarReporteRequest(
    string? Categoria,
    string? Descricao,
    decimal? Latitude,
    decimal? Longitude,
    string? Endereco,
    string? Urgencia,
    string? FotoUrl = null);

// Substituição completa: os três campos são sempre gravados (null = sem endereço / sem foto).
public record AtualizarReporteRequest(string? Descricao, string? Endereco, string? FotoUrl);

public record CategoriaResumoResponse(long Id, string Codigo, string Nome);

public record ReporteResponse(
    long Id,
    string Protocolo,
    CategoriaResumoResponse Categoria,
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

public record HistoricoStatusResponse(
    string? StatusAnterior,
    string StatusNovo,
    string? Observacao,
    DateTime CriadoEm);

public record ReporteDetalheResponse(
    long Id,
    string Protocolo,
    CategoriaResumoResponse Categoria,
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
    DateTime AtualizadoEm,
    IEnumerable<HistoricoStatusResponse> Historico);

public record FotoResponse(string FotoUrl);

// Página começa em 1.
public record PaginaResponse<T>(IEnumerable<T> Itens, int Pagina, int Tamanho, int Total, int TotalPaginas);
