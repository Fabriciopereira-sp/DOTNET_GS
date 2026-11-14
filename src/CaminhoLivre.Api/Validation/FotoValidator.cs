namespace CaminhoLivre.Api.Validation;

public static class FotoValidator
{
    public const long TamanhoMaximo = 5 * 1024 * 1024;

    private static readonly byte[] AssinaturaJpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] AssinaturaPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    // Confere o conteúdo real do arquivo (os primeiros bytes), não só a extensão.
    // Assim um .exe renomeado para .jpg é recusado.
    public static async Task<(string? Extensao, string? Erro)> ValidarAsync(
        IFormFile? foto, CancellationToken cancellationToken, string campo = "foto")
    {
        if (foto is null || foto.Length == 0)
        {
            return (null, $"Envie a foto no campo '{campo}'.");
        }

        if (foto.Length > TamanhoMaximo)
        {
            return (null, "A foto pode ter no máximo 5 MB.");
        }

        var cabecalho = new byte[8];
        await using var stream = foto.OpenReadStream();
        var lidos = await stream.ReadAtLeastAsync(cabecalho, cabecalho.Length, throwOnEndOfStream: false, cancellationToken);

        if (lidos >= AssinaturaPng.Length && cabecalho.AsSpan(0, AssinaturaPng.Length).SequenceEqual(AssinaturaPng))
        {
            return (".png", null);
        }

        if (lidos >= AssinaturaJpeg.Length && cabecalho.AsSpan(0, AssinaturaJpeg.Length).SequenceEqual(AssinaturaJpeg))
        {
            return (".jpg", null);
        }

        return (null, "Formato não aceito. Envie uma imagem JPEG ou PNG.");
    }
}
