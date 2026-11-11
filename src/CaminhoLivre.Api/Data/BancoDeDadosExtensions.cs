using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Data;

public static class BancoDeDadosExtensions
{
    // Database:Provider = "Sqlite" (padrão, roda em qualquer máquina) ou "Oracle" (banco do grupo)
    public static IServiceCollection AddBancoDeDados(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var provider = config["Database:Provider"] ?? "Sqlite";

            if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
            {
                options.UseOracle(config.GetConnectionString("Oracle"));
            }
            else
            {
                options.UseSqlite(config.GetConnectionString("Sqlite"));
            }
        });

        return services;
    }

    public static async Task InicializarBancoAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // No Oracle as tabelas são criadas pelo script-bd.sql (sem migrations, decisão D4 do grupo)
        if (!db.Database.IsSqlite())
        {
            return;
        }

        await db.Database.EnsureCreatedAsync();
        await DbSeed.PopularAsync(db);
    }
}
