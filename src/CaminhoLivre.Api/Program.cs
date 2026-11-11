using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Endpoints;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBancoDeDados();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Caminho Livre - API de Zeladoria",
        Version = "v1",
        Description = "Reportes de barreiras de acessibilidade e problemas em espaços públicos, feitos pelo cidadão e atendidos pelo órgão responsável."
    });
});

var app = builder.Build();

await app.InicializarBancoAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapCatalogoEndpoints();

app.Run();

public partial class Program { }
