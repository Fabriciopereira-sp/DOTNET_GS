using System.Text.Json;
using CaminhoLivre.Api.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CaminhoLivre.Api.Health;

public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static IServiceCollection AddVerificacoesDeSaude(this IServiceCollection services, string pastaDeFotos)
    {
        services.AddHttpClient(nameof(ApiJavaHealthCheck), c => c.Timeout = TimeSpan.FromSeconds(3));

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("banco-de-dados", tags: ["pronto"])
            .AddCheck("armazenamento-de-fotos", new ArmazenamentoHealthCheck(pastaDeFotos), tags: ["pronto"])
            .AddCheck<ApiJavaHealthCheck>("api-java", tags: ["externo"]);

        return services;
    }

    // /health      -> tudo (banco, armazenamento e API Java)
    // /health/live -> só diz se o processo da API está de pé (sem checar dependências)
    public static IEndpointRouteBuilder MapVerificacoesDeSaude(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = EscreverRespostaAsync })
            .DisableRateLimiting();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = EscreverRespostaAsync
            })
            .DisableRateLimiting();

        return app;
    }

    private static Task EscreverRespostaAsync(HttpContext http, HealthReport relatorio)
    {
        http.Response.ContentType = "application/json; charset=utf-8";

        var corpo = new
        {
            status = relatorio.Status.ToString(),
            duracaoMs = Math.Round(relatorio.TotalDuration.TotalMilliseconds, 1),
            verificacoes = relatorio.Entries.Select(e => new
            {
                nome = e.Key,
                status = e.Value.Status.ToString(),
                descricao = e.Value.Description,
                duracaoMs = Math.Round(e.Value.Duration.TotalMilliseconds, 1)
            })
        };

        return http.Response.WriteAsync(JsonSerializer.Serialize(corpo, Json));
    }
}
