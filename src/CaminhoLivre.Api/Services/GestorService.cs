using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Services;

public class GestorService(AppDbContext db, TimeProvider relogio)
{
    private DateTime Agora => relogio.GetUtcNow().UtcDateTime;

    public async Task<long?> BuscarOrgaoDoGestorAsync(long usuarioId) =>
        (await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuarioId))?.OrgaoId;

    public async Task<PaginaResponse<ReporteResponse>> ListarFilaAsync(
        long orgaoId, StatusReporte? status, Urgencia? urgencia, string? categoria, int pagina, int tamanho)
    {
        await AtualizarPrioridadesAsync(orgaoId);

        var consulta = db.Reportes.AsNoTracking().Where(r => r.OrgaoId == orgaoId);

        if (status is not null)
        {
            consulta = consulta.Where(r => r.Status == status);
        }

        if (urgencia is not null)
        {
            consulta = consulta.Where(r => r.Urgencia == urgencia);
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var codigo = categoria.Trim().ToUpper();
            consulta = consulta.Where(r => r.Categoria!.Codigo == codigo);
        }

        var total = await consulta.CountAsync();
        var itens = await consulta
            .Include(r => r.Categoria)
            .Include(r => r.Orgao)
            .OrderByDescending(r => r.Prioridade)
            .ThenBy(r => r.CriadoEm)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanho);
        return new PaginaResponse<ReporteResponse>(itens.Select(ReporteMapper.ParaResponse), pagina, tamanho, total, totalPaginas);
    }

    public Task<Reporte?> BuscarDoOrgaoAsync(long id, long orgaoId) =>
        db.Reportes
            .Include(r => r.Categoria)
            .Include(r => r.Orgao)
            .Include(r => r.Historico)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgaoId == orgaoId);

    public async Task MudarStatusAsync(Reporte reporte, StatusReporte novoStatus, string? observacao, long gestorId)
    {
        var agora = Agora;

        reporte.Historico.Add(new HistoricoStatus
        {
            StatusAnterior = reporte.Status,
            StatusNovo = novoStatus,
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            UsuarioId = gestorId,
            CriadoEm = agora
        });

        reporte.Status = novoStatus;
        reporte.AtualizadoEm = agora;
        await db.SaveChangesAsync();
    }

    // A prioridade cresce com os dias em aberto, então é recalculada antes de montar a fila.
    private async Task AtualizarPrioridadesAsync(long orgaoId)
    {
        var agora = Agora;
        var pendentes = await db.Reportes
            .Where(r => r.OrgaoId == orgaoId
                && r.Status != StatusReporte.RESOLVIDO
                && r.Status != StatusReporte.CANCELADO)
            .ToListAsync();

        foreach (var reporte in pendentes)
        {
            reporte.Prioridade = RegrasReporte.CalcularPrioridade(reporte.Urgencia, reporte.CriadoEm, agora);
        }

        await db.SaveChangesAsync();
    }
}
