using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Services;

public class ReporteService(AppDbContext db, TimeProvider relogio)
{
    public Task<Categoria?> BuscarCategoriaAsync(string codigo) =>
        db.Categorias.Include(c => c.Orgao)
            .FirstOrDefaultAsync(c => c.Codigo == codigo.Trim().ToUpper());

    public Task<bool> UsuarioExisteAsync(long usuarioId) =>
        db.Usuarios.AnyAsync(u => u.Id == usuarioId);

    public async Task<Reporte> CriarAsync(long usuarioId, Categoria categoria, CriarReporteRequest request)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;
        var urgencia = Enum.Parse<Urgencia>(request.Urgencia!, ignoreCase: true);

        var reporte = new Reporte
        {
            Protocolo = $"TMP-{Guid.NewGuid():N}"[..20],
            UsuarioId = usuarioId,
            CategoriaId = categoria.Id,
            OrgaoId = categoria.OrgaoId,
            Descricao = request.Descricao!.Trim(),
            Latitude = request.Latitude!.Value,
            Longitude = request.Longitude!.Value,
            Endereco = string.IsNullOrWhiteSpace(request.Endereco) ? null : request.Endereco.Trim(),
            Urgencia = urgencia,
            Status = StatusReporte.ABERTO,
            Prioridade = RegrasReporte.CalcularPrioridade(urgencia, agora, agora),
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        reporte.Historico.Add(new HistoricoStatus
        {
            StatusAnterior = null,
            StatusNovo = StatusReporte.ABERTO,
            Observacao = "Reporte registrado pelo cidadão.",
            UsuarioId = usuarioId,
            CriadoEm = agora
        });

        await using var transacao = await db.Database.BeginTransactionAsync();

        db.Reportes.Add(reporte);
        await db.SaveChangesAsync();

        // O protocolo usa o ID gerado pelo banco, por isso é definido depois do primeiro insert.
        reporte.Protocolo = RegrasReporte.GerarProtocolo(reporte.Id, agora.Year);
        await db.SaveChangesAsync();

        await transacao.CommitAsync();

        reporte.Categoria = categoria;
        reporte.Orgao = categoria.Orgao;
        return reporte;
    }
}
