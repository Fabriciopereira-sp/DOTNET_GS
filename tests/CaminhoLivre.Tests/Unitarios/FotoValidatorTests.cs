using CaminhoLivre.Api.Validation;
using Microsoft.AspNetCore.Http;

namespace CaminhoLivre.Tests.Unitarios;

public class FotoValidatorTests
{
    private static IFormFile CriarArquivo(byte[] conteudo, string nome) =>
        new FormFile(new MemoryStream(conteudo), 0, conteudo.Length, "foto", nome);

    [Fact]
    public async Task ValidarAsync_Jpeg_EAceito()
    {
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };

        var (extensao, erro) = await FotoValidator.ValidarAsync(CriarArquivo(jpeg, "foto.jpg"), CancellationToken.None);

        Assert.Null(erro);
        Assert.Equal(".jpg", extensao);
    }

    [Fact]
    public async Task ValidarAsync_Png_EAceito()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };

        var (extensao, erro) = await FotoValidator.ValidarAsync(CriarArquivo(png, "foto.png"), CancellationToken.None);

        Assert.Null(erro);
        Assert.Equal(".png", extensao);
    }

    [Fact]
    public async Task ValidarAsync_ExecutavelRenomeadoParaJpg_EhRecusado()
    {
        var executavel = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };

        var (extensao, erro) = await FotoValidator.ValidarAsync(CriarArquivo(executavel, "foto.jpg"), CancellationToken.None);

        Assert.Null(extensao);
        Assert.NotNull(erro);
    }

    [Fact]
    public async Task ValidarAsync_ArquivoMaiorQue5MB_EhRecusado()
    {
        var grande = new byte[FotoValidator.TamanhoMaximo + 1];
        grande[0] = 0xFF;
        grande[1] = 0xD8;
        grande[2] = 0xFF;

        var (_, erro) = await FotoValidator.ValidarAsync(CriarArquivo(grande, "grande.jpg"), CancellationToken.None);

        Assert.NotNull(erro);
    }

    [Fact]
    public async Task ValidarAsync_SemArquivo_EhRecusado()
    {
        var (_, erro) = await FotoValidator.ValidarAsync(null, CancellationToken.None);

        Assert.NotNull(erro);
    }
}
