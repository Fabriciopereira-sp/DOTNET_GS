using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace CaminhoLivre.Api.Infra;

public static class LimiteDeRequisicoesExtensions
{
    public const string PoliticaCriacaoReporte = "criacao-reporte";

    // Limites por usuário (ou por IP quando não há login), em janelas de 1 minuto.
    // Geral: 60 requisições por minuto. Criação de reporte: 5 por minuto.
    public static IServiceCollection AddLimiteDeRequisicoes(this IServiceCollection services, IConfiguration config)
    {
        var limiteGeral = config.GetValue("RateLimit:RequisicoesPorMinuto", 60);
        var limiteReportes = config.GetValue("RateLimit:ReportesPorMinuto", 5);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                RateLimitPartition.GetFixedWindowLimiter(IdentificarCliente(http), _ => Janela(limiteGeral)));

            options.AddPolicy(PoliticaCriacaoReporte, http =>
                RateLimitPartition.GetFixedWindowLimiter(IdentificarCliente(http), _ => Janela(limiteReportes)));

            options.OnRejected = async (contexto, cancellationToken) =>
            {
                var resposta = contexto.HttpContext.Response;

                if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var esperar))
                {
                    resposta.Headers.RetryAfter = ((int)Math.Ceiling(esperar.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }

                await resposta.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Muitas requisições",
                    Detail = "Limite de requisições atingido. Aguarde o tempo indicado no cabeçalho Retry-After."
                }, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
            };
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions Janela(int limite) => new()
    {
        PermitLimit = limite,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    };

    private static string IdentificarCliente(HttpContext http)
    {
        var usuarioId = http.User.FindFirst("sub")?.Value;
        return usuarioId is not null
            ? $"usuario:{usuarioId}"
            : $"ip:{http.Connection.RemoteIpAddress}";
    }
}
