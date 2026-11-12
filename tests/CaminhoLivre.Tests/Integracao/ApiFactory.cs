using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CaminhoLivre.Tests.Integracao;

// Sobe a API inteira em memória, com um banco SQLite temporário só para os testes.
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string ChaveJwtDeTeste = "chave-usada-somente-nos-testes-automatizados-0123456789";

    private readonly string _pasta = Path.Combine(Path.GetTempPath(), $"vozevia-testes-{Guid.NewGuid():N}");

    public ApiFactory()
    {
        Directory.CreateDirectory(_pasta);

        // Variáveis de ambiente são lidas pela API antes de montar os serviços.
        Environment.SetEnvironmentVariable("JWT_SECRET", ChaveJwtDeTeste);
        Environment.SetEnvironmentVariable("ConnectionStrings__Sqlite", $"Data Source={Path.Combine(_pasta, "testes.db")}");
        Environment.SetEnvironmentVariable("Uploads__Pasta", Path.Combine(_pasta, "uploads"));
    }

    public async Task<Usuario> CriarUsuarioAsync(string role, long? orgaoId = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuario = new Usuario
        {
            Nome = $"Usuário de teste {role}",
            Email = $"{Guid.NewGuid():N}@teste.com",
            Role = role,
            OrgaoId = orgaoId
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    public async Task<long> ObterOrgaoIdAsync(AreaOrgao area)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (await db.Orgaos.FirstAsync(o => o.Area == area)).Id;
    }

    public string GerarToken(Usuario usuario)
    {
        using var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TokenService>().Gerar(usuario).Token;
    }

    public async Task<HttpClient> ClienteAutenticadoAsync(string role, long? orgaoId = null)
    {
        var usuario = await CriarUsuarioAsync(role, orgaoId);
        return ClienteComToken(GerarToken(usuario));
    }

    public HttpClient ClienteComToken(string token)
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return cliente;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            Directory.Delete(_pasta, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // O SQLite pode manter o arquivo aberto por alguns instantes; a pasta temporária é limpa pelo sistema.
        }
    }
}

[CollectionDefinition(Nome)]
public class ColecaoIntegracao : ICollectionFixture<ApiFactory>
{
    public const string Nome = "Integracao";
}
