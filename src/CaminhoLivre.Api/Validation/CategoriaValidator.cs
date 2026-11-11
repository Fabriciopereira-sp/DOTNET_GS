using System.Text.RegularExpressions;
using CaminhoLivre.Api.Contracts;

namespace CaminhoLivre.Api.Validation;

public static partial class CategoriaValidator
{
    [GeneratedRegex("^[A-Z][A-Z_]{1,29}$")]
    private static partial Regex FormatoCodigo();

    public static Dictionary<string, string[]> Validar(CategoriaRequest request)
    {
        var erros = new Dictionary<string, string[]>();

        var codigo = request.Codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!FormatoCodigo().IsMatch(codigo))
        {
            erros["codigo"] = ["Código com 2 a 30 caracteres, só letras e underline (ex.: PISO_TATIL)."];
        }

        var nome = request.Nome?.Trim() ?? string.Empty;
        if (nome.Length is < 3 or > 80)
        {
            erros["nome"] = ["O nome deve ter entre 3 e 80 caracteres."];
        }

        if (request.OrgaoId is null or <= 0)
        {
            erros["orgaoId"] = ["Informe o órgão responsável pela categoria."];
        }

        return erros;
    }
}
