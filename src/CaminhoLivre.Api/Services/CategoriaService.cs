using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Services;

public class CategoriaService(AppDbContext db)
{
    public async Task<List<CategoriaResponse>> ListarAsync()
    {
        var categorias = await db.Categorias.AsNoTracking()
            .Include(c => c.Orgao)
            .OrderBy(c => c.Id)
            .ToListAsync();

        return categorias.Select(ParaResponse).ToList();
    }

    public Task<Categoria?> BuscarAsync(long id) =>
        db.Categorias.Include(c => c.Orgao).FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> OrgaoExisteAsync(long orgaoId) =>
        db.Orgaos.AnyAsync(o => o.Id == orgaoId);

    public Task<bool> CodigoEmUsoAsync(string codigo, long? ignorarId = null) =>
        db.Categorias.AnyAsync(c => c.Codigo == codigo && c.Id != ignorarId);

    public Task<bool> PossuiReportesAsync(long categoriaId) =>
        db.Reportes.AnyAsync(r => r.CategoriaId == categoriaId);

    public async Task<Categoria> CriarAsync(CategoriaRequest request)
    {
        var categoria = new Categoria();
        Preencher(categoria, request);

        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();
        await db.Entry(categoria).Reference(c => c.Orgao).LoadAsync();
        return categoria;
    }

    public async Task AtualizarAsync(Categoria categoria, CategoriaRequest request)
    {
        Preencher(categoria, request);
        await db.SaveChangesAsync();
        await db.Entry(categoria).Reference(c => c.Orgao).LoadAsync();
    }

    public async Task ExcluirAsync(Categoria categoria)
    {
        db.Categorias.Remove(categoria);
        await db.SaveChangesAsync();
    }

    public static string NormalizarCodigo(string? codigo) => codigo?.Trim().ToUpperInvariant() ?? string.Empty;

    public static CategoriaResponse ParaResponse(Categoria c) =>
        new(c.Id, c.Codigo, c.Nome, c.OrgaoId, c.Orgao?.Nome ?? string.Empty);

    private static void Preencher(Categoria categoria, CategoriaRequest request)
    {
        categoria.Codigo = NormalizarCodigo(request.Codigo);
        categoria.Nome = request.Nome!.Trim();
        categoria.OrgaoId = request.OrgaoId!.Value;
    }
}
