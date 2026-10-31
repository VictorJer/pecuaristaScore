namespace ScoreRebanho.Web.Models;
public record ScoreResumoVm(Guid Id, string Autor, DateTime CriadoEmUtc, int Score, string Classificacao);
public record HistoricoVm(int Pagina, IReadOnlyList<ScoreResumoVm> Scores);
