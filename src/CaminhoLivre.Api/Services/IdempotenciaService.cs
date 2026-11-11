using System.Security.Cryptography;
using System.Text.Json;
using CaminhoLivre.Api.Data;
using CaminhoLivre.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Services;

public enum SituacaoIdempotencia
{
    Nova,
    Repetida,
    CorpoDiferente,
    EmProcessamento
}

public record ResultadoIdempotencia(SituacaoIdempotencia Situacao, RegistroIdempotencia? Registro = null);

public class IdempotenciaService(AppDbContext db, TimeProvider relogio)
{
    public const string Cabecalho = "Idempotency-Key";
    private static readonly TimeSpan Validade = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string CalcularHash<T>(T corpo) =>
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(corpo, Json)));

    // Reserva a chave antes de processar. Se a chave já existe, decide o que fazer com a repetição.
    public async Task<ResultadoIdempotencia> ReservarAsync(string chave, long usuarioId, string hashCorpo)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;
        var existente = await db.RegistrosIdempotencia
            .FirstOrDefaultAsync(r => r.Chave == chave && r.UsuarioId == usuarioId);

        if (existente is not null && agora - existente.CriadoEm > Validade)
        {
            db.RegistrosIdempotencia.Remove(existente);
            await db.SaveChangesAsync();
            existente = null;
        }

        if (existente is not null)
        {
            return Classificar(existente, hashCorpo);
        }

        var registro = new RegistroIdempotencia
        {
            Chave = chave,
            UsuarioId = usuarioId,
            HashCorpo = hashCorpo,
            StatusHttp = 0,
            CriadoEm = agora
        };

        db.RegistrosIdempotencia.Add(registro);
        try
        {
            await db.SaveChangesAsync();
            return new ResultadoIdempotencia(SituacaoIdempotencia.Nova, registro);
        }
        catch (DbUpdateException)
        {
            // Outra requisição com a mesma chave gravou primeiro (envio duplicado ao mesmo tempo).
            db.Entry(registro).State = EntityState.Detached;
            var concorrente = await db.RegistrosIdempotencia.AsNoTracking()
                .FirstAsync(r => r.Chave == chave && r.UsuarioId == usuarioId);
            return Classificar(concorrente, hashCorpo);
        }
    }

    public async Task ConcluirAsync(RegistroIdempotencia registro, int statusHttp, object resposta)
    {
        registro.StatusHttp = statusHttp;
        registro.RespostaJson = JsonSerializer.Serialize(resposta, Json);
        await db.SaveChangesAsync();
    }

    // Se o processamento falhar, libera a chave para o app poder tentar de novo.
    public async Task LiberarAsync(RegistroIdempotencia registro)
    {
        db.ChangeTracker.Clear();
        db.RegistrosIdempotencia.Remove(registro);
        await db.SaveChangesAsync();
    }

    private static ResultadoIdempotencia Classificar(RegistroIdempotencia registro, string hashCorpo)
    {
        if (registro.HashCorpo != hashCorpo)
        {
            return new ResultadoIdempotencia(SituacaoIdempotencia.CorpoDiferente);
        }

        return registro.StatusHttp == 0
            ? new ResultadoIdempotencia(SituacaoIdempotencia.EmProcessamento)
            : new ResultadoIdempotencia(SituacaoIdempotencia.Repetida, registro);
    }
}
