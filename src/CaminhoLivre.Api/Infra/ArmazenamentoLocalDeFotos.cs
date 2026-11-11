namespace CaminhoLivre.Api.Infra;

public class ArmazenamentoLocalDeFotos(string pasta) : IArmazenamentoDeFotos
{
    public const string CaminhoPublico = "/uploads";

    public async Task<string> SalvarAsync(Stream conteudo, string extensao, CancellationToken cancellationToken)
    {
        // Nome gerado pelo servidor: nunca usamos o nome enviado pelo usuário.
        var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
        var caminho = Path.Combine(pasta, nomeArquivo);

        await using var arquivo = File.Create(caminho);
        await conteudo.CopyToAsync(arquivo, cancellationToken);

        return $"{CaminhoPublico}/{nomeArquivo}";
    }

    public Task ExcluirAsync(string url, CancellationToken cancellationToken)
    {
        var nomeArquivo = Path.GetFileName(url);
        var caminho = Path.Combine(pasta, nomeArquivo);

        if (File.Exists(caminho))
        {
            File.Delete(caminho);
        }

        return Task.CompletedTask;
    }
}
