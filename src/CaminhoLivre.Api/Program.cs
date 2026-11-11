using CaminhoLivre.Api.Auth;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Endpoints;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBancoDeDados();
builder.AddAutenticacaoJwt();
builder.Services.AddScoped<TokenService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Caminho Livre - API de Zeladoria",
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapCatalogoEndpoints();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Auth:HabilitarTokenDeTeste"))
{
    app.MapTokenTesteEndpoints();
}

app.Run();

public partial class Program { }
