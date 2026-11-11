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
}
