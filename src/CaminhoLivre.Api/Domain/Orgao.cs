namespace CaminhoLivre.Api.Domain;

public class Orgao
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public AreaOrgao Area { get; set; }

    public List<Categoria> Categorias { get; set; } = [];
}
