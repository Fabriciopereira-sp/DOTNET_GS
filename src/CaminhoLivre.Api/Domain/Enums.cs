namespace CaminhoLivre.Api.Domain;

public enum AreaOrgao
{
    OBRAS,
    ILUMINACAO,
    MOBILIDADE,
    LIMPEZA
}

public enum Urgencia
{
    BAIXA,
    MEDIA,
    ALTA
}

public enum StatusReporte
{
    ABERTO,
    EM_ANALISE,
    EM_EXECUCAO,
    RESOLVIDO,
    CANCELADO
}
