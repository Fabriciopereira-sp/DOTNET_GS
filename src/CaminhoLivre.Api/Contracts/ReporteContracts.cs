namespace CaminhoLivre.Api.Contracts;

public record CriarReporteRequest(
    string? Categoria,
    string? Descricao,
    decimal? Latitude,
    decimal? Longitude,
    string? Endereco,
    string? Urgencia);

public record AtualizarReporteRequest(string? Descricao, string? Endereco);

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

public record HistoricoStatusResponse(
    string? StatusAnterior,
    string StatusNovo,
    string? Observacao,
    DateTime CriadoEm);

public record ReporteDetalheResponse(ReporteResponse Reporte, IEnumerable<HistoricoStatusResponse> Historico);

public record PaginaResponse<T>(IEnumerable<T> Itens, int Pagina, int Tamanho, int TotalItens, int TotalPaginas);
