using FluentResults;
namespace ScoreRebanho.Application;
public record ScoreSalvoDto(Guid Id, string UsuarioId, DateTime CriadoEmUtc, EntradaScoreDto Entrada, ResultadoScoreDto Resultado);
public interface IHistoricoScores
{
    Task SalvarAsync(ScoreSalvoDto score);
    Task<IReadOnlyList<ScoreSalvoDto>> ListarAsync(int pagina);
    Task<ScoreSalvoDto?> ObterAsync(Guid id);
}
public sealed class ConsultarScores(IHistoricoScores historico)
{
    public async Task<Result<IReadOnlyList<ScoreSalvoDto>>> ListarAsync(int pagina) =>
        Result.Ok(await historico.ListarAsync(Math.Max(1, pagina)));
    public async Task<Result<ScoreSalvoDto>> ObterAsync(Guid id)
    {
        var score = await historico.ObterAsync(id);
        return score is null ? Result.Fail<ScoreSalvoDto>("Score não encontrado.") : Result.Ok(score);
    }
}
public sealed class GerarScore(CalcularScoreRebanho calculadora, IHistoricoScores historico)
{
    public async Task<Result<ResultadoScoreDto>> ExecutarAsync(EntradaScoreDto entrada, string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId)) return Result.Fail<ResultadoScoreDto>("Usuário não identificado.");
        var resultado = calculadora.Executar(entrada);
        if (resultado.IsFailed) return resultado;
        await historico.SalvarAsync(new(Guid.NewGuid(), usuarioId, DateTime.UtcNow, entrada, resultado.Value));
        return resultado;
    }
}
