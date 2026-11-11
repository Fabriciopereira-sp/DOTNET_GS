using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CaminhoLivre.Api.Health;

// Serviço externo: a API Java, que emite o token de login.
// Se ela cair, esta API continua funcionando com tokens já emitidos, por isso o estado é "Degraded" e não "Unhealthy".
public class ApiJavaHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var url = config["ServicosExternos:ApiJavaHealthUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            return HealthCheckResult.Healthy("Endereço da API Java não configurado neste ambiente.");
        }

        try
        {
            var cliente = httpClientFactory.CreateClient(nameof(ApiJavaHealthCheck));
            using var resposta = await cliente.GetAsync(url, cancellationToken);

            return resposta.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("API Java respondendo.")
                : HealthCheckResult.Degraded($"API Java respondeu {(int)resposta.StatusCode}.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return HealthCheckResult.Degraded("API Java fora do ar ou sem resposta.");
        }
    }
}
