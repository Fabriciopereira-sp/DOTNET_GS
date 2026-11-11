using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Api.Services;

public static class ReporteMapper
{
    public static ReporteResponse ParaResponse(Reporte r) => new(
        r.Id,
        r.Protocolo,
        r.Categoria?.Codigo ?? string.Empty,
        r.Categoria?.Nome ?? string.Empty,
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
        ParaResponse(r),
        r.Historico
            .OrderBy(h => h.CriadoEm)
            .ThenBy(h => h.Id)
            .Select(h => new HistoricoStatusResponse(
                h.StatusAnterior?.ToString(),
                h.StatusNovo.ToString(),
                h.Observacao,
                h.CriadoEm)));
}
