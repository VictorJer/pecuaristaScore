using ScoreRebanho.Domain;
namespace ScoreRebanho.Application.Tests;
[TestClass]
public class HistoricoTests
{
    private sealed class PremissasTeste : IProvedorPremissas { public Premissas Obter() => new(); }
    private sealed class Memoria : IHistoricoScores
    {
        public List<ScoreSalvoDto> Scores { get; } = [];
        public Task SalvarAsync(ScoreSalvoDto score) { Scores.Add(score); return Task.CompletedTask; }
        public Task<IReadOnlyList<ScoreSalvoDto>> ListarAsync(int pagina) => Task.FromResult<IReadOnlyList<ScoreSalvoDto>>(Scores);
        public Task<ScoreSalvoDto?> ObterAsync(Guid id) => Task.FromResult(Scores.SingleOrDefault(s=>s.Id==id));
    }
    [TestMethod] public async Task SalvaResultadoComAutorEPremissas()
    {
        var banco=new Memoria(); var servico=new GerarScore(new(new PremissasTeste()),banco);
        var entrada=new EntradaScoreDto { Area=10,Animais=10,PesoCompra=450,PrecoCompra=5,Dias=180 };
        var resultado=await servico.ExecutarAsync(entrada,"usuario-teste");
        Assert.IsTrue(resultado.IsSuccess); Assert.AreEqual(1,banco.Scores.Count);
        Assert.AreEqual("usuario-teste",banco.Scores[0].UsuarioId);
        Assert.AreEqual(resultado.Value,banco.Scores[0].Resultado);
        Assert.AreEqual(entrada,banco.Scores[0].Entrada);
    }
    [TestMethod] public async Task NaoSalvaEntradaInvalidaOuSemAutor()
    {
        var banco=new Memoria(); var servico=new GerarScore(new(new PremissasTeste()),banco);
        Assert.IsTrue((await servico.ExecutarAsync(new(),"usuario")).IsFailed);
        Assert.IsTrue((await servico.ExecutarAsync(new(),"")).IsFailed);
        Assert.AreEqual(0,banco.Scores.Count);
        Assert.IsTrue((await new ConsultarScores(banco).ObterAsync(Guid.NewGuid())).IsFailed);
    }
}
