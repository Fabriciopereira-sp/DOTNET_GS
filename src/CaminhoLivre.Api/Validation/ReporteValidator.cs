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

        ValidarDescricao(request.Descricao, erros);

        if (request.Latitude is null or < -90m or > 90m)
        {
            erros["latitude"] = ["Latitude obrigatória, entre -90 e 90."];
        }

        if (request.Longitude is null or < -180m or > 180m)
        {
            erros["longitude"] = ["Longitude obrigatória, entre -180 e 180."];
        }

        ValidarEndereco(request.Endereco, erros);

        if (!Enum.TryParse<Urgencia>(request.Urgencia, ignoreCase: true, out _) || int.TryParse(request.Urgencia, out _))
        {
            erros["urgencia"] = ["Urgência deve ser BAIXA, MEDIA ou ALTA."];
        }

        return erros;
    }

    public static Dictionary<string, string[]> Validar(AtualizarReporteRequest request)
    {
        var erros = new Dictionary<string, string[]>();
        ValidarDescricao(request.Descricao, erros);
        ValidarEndereco(request.Endereco, erros);
        return erros;
    }

    public static Dictionary<string, string[]> ValidarPaginacao(int pagina, int tamanho)
    {
        var erros = new Dictionary<string, string[]>();

        if (pagina < 0)
        {
            erros["page"] = ["A página começa em 0."];
        }

        if (tamanho is < 1 or > 50)
        {
            erros["size"] = ["O tamanho da página deve ser entre 1 e 50."];
        }

        return erros;
    }

    private static void ValidarDescricao(string? descricao, Dictionary<string, string[]> erros)
    {
        var texto = descricao?.Trim() ?? string.Empty;
        if (texto.Length < 10 || texto.Length > 500)
        {
            erros["descricao"] = ["A descrição deve ter entre 10 e 500 caracteres."];
        }
    }

    private static void ValidarEndereco(string? endereco, Dictionary<string, string[]> erros)
    {
        if (endereco?.Length > 200)
        {
            erros["endereco"] = ["O endereço pode ter no máximo 200 caracteres."];
        }
    }
}
