using System.Text.Json.Serialization;
using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Endpoints;
using CaminhoLivre.Api.Health;
using CaminhoLivre.Api.Infra;
using CaminhoLivre.Api.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBancoDeDados();
builder.AddAutenticacaoJwt();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ReporteService>();
builder.Services.AddScoped<GestorService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<IdempotenciaService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddLimiteDeRequisicoes(builder.Configuration);

var pastaDeFotos = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Uploads:Pasta"] ?? "uploads");
Directory.CreateDirectory(pastaDeFotos);
builder.Services.AddSingleton<IArmazenamentoDeFotos>(new ArmazenamentoLocalDeFotos(pastaDeFotos));

builder.Services.AddVerificacoesDeSaude(pastaDeFotos);

// Erros sempre no formato ProblemDetails (RFC 7807). Fora de desenvolvimento, sem stack trace para o usuário.
builder.Services.AddProblemDetails();

// JSON em camelCase (padrão) e enums sempre como texto ("ALTA", "ABERTO").
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Voz & Via - API de Zeladoria",
        Version = "v1",
        Description = "Reportes de barreiras de acessibilidade e problemas em espaços públicos, feitos pelo cidadão e atendidos pelo órgão responsável."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole somente o token JWT (sem a palavra Bearer)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

await app.InicializarBancoAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(pastaDeFotos),
    RequestPath = ArmazenamentoLocalDeFotos.CaminhoPublico
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Voz & Via - API de Zeladoria v1");
    options.DocumentTitle = "Voz & Via - API de Zeladoria";
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapCatalogoEndpoints();
app.MapReporteEndpoints();
app.MapFotoEndpoints();
app.MapGestorEndpoints();
app.MapVerificacoesDeSaude();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Auth:HabilitarTokenDeTeste"))
{
    app.MapTokenTesteEndpoints();
}

app.Run();

public partial class Program { }
