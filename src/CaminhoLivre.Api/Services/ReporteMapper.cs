using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Api.Services;

public static class ReporteMapper
{
    public static ReporteResponse ParaResponse(Reporte r) => new(
        r.Id,
        r.Protocolo,
        Categoria(r),
        r.Orgao?.Nome ?? string.Empty,
        r.Descricao,
        r.Latitude,
        r.Longitude,
        r.Endereco,
        r.FotoUrl,
        r.Urgencia.ToString(),
        r.Status.ToString(),
        r.Prioridade,
        r.CriadoEm,
        r.AtualizadoEm);

    public static ReporteDetalheResponse ParaDetalhe(Reporte r) => new(
        r.Id,
        r.Protocolo,
        Categoria(r),
        r.Orgao?.Nome ?? string.Empty,
        r.Descricao,
        r.Latitude,
        r.Longitude,
        r.Endereco,
        r.FotoUrl,
        r.Urgencia.ToString(),
        r.Status.ToString(),
        r.Prioridade,
        r.CriadoEm,
        r.AtualizadoEm,
        r.Historico
            .OrderBy(h => h.CriadoEm)
            .ThenBy(h => h.Id)
            .Select(h => new HistoricoStatusResponse(
                h.StatusAnterior?.ToString(),
                h.StatusNovo.ToString(),
                h.Observacao,
                h.CriadoEm))
            .ToList());

    private static CategoriaResumoResponse Categoria(Reporte r) =>
        new(r.CategoriaId, r.Categoria?.Codigo ?? string.Empty, r.Categoria?.Nome ?? string.Empty);
}
