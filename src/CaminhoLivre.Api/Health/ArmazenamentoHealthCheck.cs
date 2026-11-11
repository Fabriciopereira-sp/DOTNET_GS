using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CaminhoLivre.Api.Health;

// Confere se a pasta das fotos existe e aceita gravação.
public class ArmazenamentoHealthCheck(string pasta) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var teste = Path.Combine(pasta, $".health-{Guid.NewGuid():N}");
            File.WriteAllText(teste, "ok");
            File.Delete(teste);
            return Task.FromResult(HealthCheckResult.Healthy("Pasta de fotos com permissão de escrita."));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Não foi possível gravar na pasta de fotos."));
        }
    }
}
