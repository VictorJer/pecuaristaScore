using ScoreRebanho.Domain;

namespace ScoreRebanho.Application.Tests;

[TestClass]
public class ResumoTests
{
    private sealed class Provedor : IProvedorPremissas { public Premissas Obter() => new(); }
    private static EntradaScoreDto Entrada => new() { Area = 10, Animais = 10, PesoCompra = 450, PrecoCompra = 5, Dias = 180 };

    [TestMethod]
    [DataRow(2, 10)]
    [DataRow(3, 13)]
    [DataRow(20, 16)]
    public void ResumoUsaCapacidadeDoRelatorio(int piquetes, int capacidade)
    {
        var servico = new CalcularScoreRebanho(new Provedor());
        var entrada = Entrada with { Rotacao = true, Piquetes = piquetes };
        var resumo = servico.Resumir(entrada);
        Assert.IsTrue(resumo.IsSuccess);
        Assert.AreEqual(10m, resumo.Value.UnidadesAnimais);
        Assert.AreEqual((decimal)capacidade, resumo.Value.Capacidade);
        Assert.AreEqual(servico.Executar(entrada).Value.Indicadores.Single(i => i.Nome == "Ocupação").Valor, resumo.Value.Ocupacao);
    }

    [TestMethod]
    public void ResumoRejeitaEntradaInvalidaEOverflow()
    {
        var servico = new CalcularScoreRebanho(new Provedor());
        var invalido = servico.Resumir(Entrada with { Area = 0 });
        Assert.IsTrue(invalido.IsFailed);
        Assert.AreEqual("Area", invalido.Errors[0].Metadata["Campo"]);
        Assert.IsTrue(servico.Resumir(Entrada with { PesoCompra = decimal.MaxValue }).IsFailed);
    }

    [TestMethod]
    [DataRow(4, 40)]
    [DataRow(10, 100)]
    [DataRow(20, 200)]
    public void ResumoPreservaOcupacaoSemLimitarPercentual(int animais, int ocupacao)
    {
        var resumo = new CalcularScoreRebanho(new Provedor()).Resumir(Entrada with { Animais = animais });
        Assert.IsTrue(resumo.IsSuccess);
        Assert.AreEqual((decimal)ocupacao, resumo.Value.Ocupacao);
    }
}
