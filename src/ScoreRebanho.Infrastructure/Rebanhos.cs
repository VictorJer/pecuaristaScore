using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ScoreRebanho.Application;
using ScoreRebanho.Infrastructure.Data;

namespace ScoreRebanho.Infrastructure;

public sealed class RebanhoPersistido
{
    public Guid Id { get; set; }
    public Guid Versao { get; set; }
    public string UsuarioId { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Propriedade { get; set; } = "";
    public DateTime CriadoEmUtc { get; set; }
    public DateTime AtualizadoEmUtc { get; set; }
    public string EntradaJson { get; set; } = "";
    public string ResultadoJson { get; set; } = "";
}
public sealed class Rebanhos(ApplicationDbContext db) : IRebanhos
{
    public async Task<IReadOnlyList<RebanhoDto>> ListarAsync(string usuarioId, int pagina)
    {
        var itens = await db.Set<RebanhoPersistido>().AsNoTracking().Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.AtualizadoEmUtc).ThenBy(r => r.Id)
            .Skip((Math.Clamp(pagina, 1, 1000000) - 1) * 20).Take(21).ToListAsync();
        return itens.Select(Mapear).ToArray();
    }
    public async Task<RebanhoDto?> ObterAsync(string usuarioId, Guid id)
    {
        var item = await db.Set<RebanhoPersistido>().AsNoTracking().SingleOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);
        return item is null ? null : Mapear(item);
    }
    public async Task<bool> SalvarAsync(string usuarioId, RebanhoDto rebanho, Guid? versaoAnterior)
    {
        var entrada = JsonSerializer.Serialize(rebanho.Entrada);
        var resultado = JsonSerializer.Serialize(rebanho.Resultado);
        await using var transacao = await db.Database.BeginTransactionAsync();
        if (versaoAnterior.HasValue)
        {
            var alterados = await db.Set<RebanhoPersistido>()
                .Where(r => r.Id == rebanho.Id && r.UsuarioId == usuarioId && r.Versao == versaoAnterior.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Nome, rebanho.Nome).SetProperty(r => r.Propriedade, rebanho.Propriedade)
                    .SetProperty(r => r.Versao, rebanho.Versao).SetProperty(r => r.AtualizadoEmUtc, rebanho.AtualizadoEmUtc)
                    .SetProperty(r => r.EntradaJson, entrada).SetProperty(r => r.ResultadoJson, resultado));
            if (alterados == 0) return false;
        }
        else db.Set<RebanhoPersistido>().Add(new() { Id = rebanho.Id, Versao = rebanho.Versao, UsuarioId = usuarioId,
            Nome = rebanho.Nome, Propriedade = rebanho.Propriedade, CriadoEmUtc = rebanho.CriadoEmUtc,
            AtualizadoEmUtc = rebanho.AtualizadoEmUtc, EntradaJson = entrada, ResultadoJson = resultado });
        // Cadastro e snapshot do cálculo são gravados na mesma transação.
        db.Set<ScorePersistido>().Add(new() { Id = Guid.NewGuid(), UsuarioId = usuarioId, CriadoEmUtc = rebanho.AtualizadoEmUtc,
            EntradaJson = entrada, ResultadoJson = resultado });
        await db.SaveChangesAsync();
        await transacao.CommitAsync();
        return true;
    }
    public async Task<bool> ExcluirAsync(string usuarioId, Guid id, Guid versao) =>
        await db.Set<RebanhoPersistido>().Where(r => r.Id == id && r.UsuarioId == usuarioId && r.Versao == versao).ExecuteDeleteAsync() == 1;
    private static RebanhoDto Mapear(RebanhoPersistido r) => new(r.Id, r.Versao, r.Nome, r.Propriedade, r.CriadoEmUtc,
        r.AtualizadoEmUtc, JsonSerializer.Deserialize<EntradaScoreDto>(r.EntradaJson)!, JsonSerializer.Deserialize<ResultadoScoreDto>(r.ResultadoJson)!);
}
