using System.Text.RegularExpressions;
using CaminhoLivre.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CaminhoLivre.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Orgao> Orgaos => Set<Orgao>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Reporte> Reportes => Set<Reporte>();
    public DbSet<HistoricoStatus> HistoricosStatus => Set<HistoricoStatus>();
    public DbSet<RegistroIdempotencia> RegistrosIdempotencia => Set<RegistroIdempotencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("VV_USUARIO");
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Orgao).WithMany().HasForeignKey(x => x.OrgaoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Orgao>(e =>
        {
            e.ToTable("VV_ORGAO");
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Area).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("VV_CATEGORIA");
            e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Nome).HasMaxLength(80).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.HasOne(x => x.Orgao).WithMany(o => o.Categorias).HasForeignKey(x => x.OrgaoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reporte>(e =>
        {
            e.ToTable("VV_REPORTE");
            e.Property(x => x.Protocolo).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Protocolo).IsUnique();
            e.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
            e.Property(x => x.Endereco).HasMaxLength(200);
            e.Property(x => x.FotoUrl).HasMaxLength(500);
            e.Property(x => x.Latitude).HasPrecision(9, 6);
            e.Property(x => x.Longitude).HasPrecision(9, 6);
            e.Property(x => x.Urgencia).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Orgao).WithMany().HasForeignKey(x => x.OrgaoId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Historico).WithOne(h => h.Reporte).HasForeignKey(h => h.ReporteId);
        });

        modelBuilder.Entity<HistoricoStatus>(e =>
        {
            e.ToTable("VV_HISTORICO_STATUS");
            e.Property(x => x.StatusAnterior).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.StatusNovo).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Observacao).HasMaxLength(300);
        });

        modelBuilder.Entity<RegistroIdempotencia>(e =>
        {
            e.ToTable("VV_IDEMPOTENCIA");
            e.HasKey(x => new { x.Chave, x.UsuarioId });
            e.Property(x => x.Chave).HasMaxLength(100);
            e.Property(x => x.HashCorpo).HasMaxLength(64).IsRequired();
        });

        // Datas são gravadas em UTC. Ao ler, marca como UTC para o JSON sair no formato ISO 8601 com "Z".
        var utc = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        foreach (var propriedade in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetProperties())
                     .Where(p => p.ClrType == typeof(DateTime)))
        {
            propriedade.SetValueConverter(utc);
        }

        // Padrão do banco do grupo: colunas em maiúsculas separadas por underline (ex.: ORGAO_ID)
        foreach (var entidade in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var propriedade in entidade.GetProperties())
            {
                propriedade.SetColumnName(ParaPadraoDoBanco(propriedade.Name));
            }
        }
    }

    private static string ParaPadraoDoBanco(string nome) =>
        Regex.Replace(nome, "(?<=[a-z0-9])([A-Z])", "_$1").ToUpperInvariant();
}
