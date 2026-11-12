using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Data;

public static class BancoDeDadosExtensions
{
    // Database:Provider escolhe o banco sem mudar o código:
    //   "Sqlite"    padrão, roda em qualquer máquina sem instalar nada
    //   "Oracle"    banco do grupo (tabelas criadas pelo script-bd.sql)
    //   "SqlServer" Azure SQL, opção para o deploy na nuvem
    public static IServiceCollection AddBancoDeDados(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var provider = config["Database:Provider"] ?? "Sqlite";

            switch (provider.ToLowerInvariant())
            {
                case "oracle":
                    options.UseOracle(ConexaoObrigatoria(config, "Oracle"));
                    break;
                case "sqlserver":
                    // Sem EnableRetryOnFailure: a criação do reporte usa transação própria,
                    // e a estratégia de nova tentativa do SQL Server não aceita transações abertas pelo código.
                    options.UseSqlServer(ConexaoObrigatoria(config, "SqlServer"));
                    break;
                default:
                    options.UseSqlite(config.GetConnectionString("Sqlite"));
                    break;
            }
        });

        return services;
    }

    public static async Task InicializarBancoAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // SQLite: sempre cria as tabelas e os dados de teste.
        // Oracle e SQL Server: só se Database:CriarTabelasAoIniciar = true. No Oracle do grupo as
        // tabelas vêm do script-bd.sql (sem migrations, decisão D4).
        var criar = db.Database.IsSqlite() || app.Configuration.GetValue<bool>("Database:CriarTabelasAoIniciar");
        if (!criar)
        {
            return;
        }

        await db.Database.EnsureCreatedAsync();
        await DbSeed.PopularAsync(db);
    }

    private static string ConexaoObrigatoria(IConfiguration config, string nome) =>
        config.GetConnectionString(nome) is { Length: > 0 } conexao
            ? conexao
            : throw new InvalidOperationException($"Configure a variável de ambiente ConnectionStrings__{nome}.");
}
