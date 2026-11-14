namespace CaminhoLivre.Api.Endpoints;

// Erros no formato ProblemDetails, com "detail" em português para o app mostrar ao usuário.
public static class Respostas
{
    public static IResult NaoEncontrado(string detalhe) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Não encontrado", detail: detalhe);

    public static IResult Conflito(string detalhe, string titulo = "Operação não permitida") =>
        Results.Problem(statusCode: StatusCodes.Status409Conflict, title: titulo, detail: detalhe);
}
