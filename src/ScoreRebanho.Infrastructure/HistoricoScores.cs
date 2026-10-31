using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ScoreRebanho.Application;
using ScoreRebanho.Infrastructure.Data;
namespace ScoreRebanho.Infrastructure;
public sealed class ScorePersistido
{
    public Guid Id { get; set; }
    public string UsuarioId { get; set; } = "";
    public DateTime CriadoEmUtc { get; set; }
    public string EntradaJson { get; set; } = "";
    public string ResultadoJson { get; set; } = "";
}
public sealed class HistoricoScores(ApplicationDbContext db) : IHistoricoScores
{
    public async Task SalvarAsync(ScoreSalvoDto score)
    {
        db.Set<ScorePersistido>().Add(new() { Id=score.Id, UsuarioId=score.UsuarioId, CriadoEmUtc=score.CriadoEmUtc,
            EntradaJson=JsonSerializer.Serialize(score.Entrada), ResultadoJson=JsonSerializer.Serialize(score.Resultado) });
        await db.SaveChangesAsync();
    }
    public async Task<IReadOnlyList<ScoreSalvoDto>> ListarAsync(int pagina)
    {
        var registros=await db.Set<ScorePersistido>().AsNoTracking().OrderByDescending(s=>s.CriadoEmUtc).ThenBy(s=>s.Id)
            .Skip((Math.Clamp(pagina,1,1000000)-1)*20).Take(20).ToListAsync();
        return registros.Select(Mapear).ToArray();
    }
    public async Task<ScoreSalvoDto?> ObterAsync(Guid id)
    {
        var registro=await db.Set<ScorePersistido>().AsNoTracking().SingleOrDefaultAsync(s=>s.Id==id);
        return registro is null ? null : Mapear(registro);
    }
    private static ScoreSalvoDto Mapear(ScorePersistido r) => new(r.Id,r.UsuarioId,r.CriadoEmUtc,
        JsonSerializer.Deserialize<EntradaScoreDto>(r.EntradaJson)!, JsonSerializer.Deserialize<ResultadoScoreDto>(r.ResultadoJson)!);
}
