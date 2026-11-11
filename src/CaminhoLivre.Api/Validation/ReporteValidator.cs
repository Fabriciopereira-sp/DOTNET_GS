using CaminhoLivre.Api.Contracts;
using CaminhoLivre.Api.Domain;

namespace CaminhoLivre.Api.Validation;

public static class ReporteValidator
{
    public static Dictionary<string, string[]> Validar(CriarReporteRequest request)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Categoria))
        {
            erros["categoria"] = ["Informe a categoria do problema."];
        }

        var descricao = request.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length < 10 || descricao.Length > 500)
        {
            erros["descricao"] = ["A descrição deve ter entre 10 e 500 caracteres."];
        }

        if (request.Latitude is null or < -90m or > 90m)
        {
            erros["latitude"] = ["Latitude obrigatória, entre -90 e 90."];
        }

        if (request.Longitude is null or < -180m or > 180m)
        {
            erros["longitude"] = ["Longitude obrigatória, entre -180 e 180."];
        }

        if (request.Endereco?.Length > 200)
        {
            erros["endereco"] = ["O endereço pode ter no máximo 200 caracteres."];
        }

        if (!Enum.TryParse<Urgencia>(request.Urgencia, ignoreCase: true, out _) || int.TryParse(request.Urgencia, out _))
        {
            erros["urgencia"] = ["Urgência deve ser BAIXA, MEDIA ou ALTA."];
        }

        return erros;
    }
}
