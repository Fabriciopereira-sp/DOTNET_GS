namespace CaminhoLivre.Api.Infra;

// Abstração para guardar as fotos fora do banco (decisão D9). Hoje grava em pasta local;
// na nuvem pode ser trocada por uma implementação com Azure Blob Storage sem mudar os endpoints.
public interface IArmazenamentoDeFotos
{
    Task<string> SalvarAsync(Stream conteudo, string extensao, CancellationToken cancellationToken);
    Task ExcluirAsync(string url, CancellationToken cancellationToken);
}
