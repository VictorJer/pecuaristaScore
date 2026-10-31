using ScoreRebanho.Domain;

namespace ScoreRebanho.Domain.Tests;

[TestClass]
public class AlertasTests
{
    private static Entrada Entrada => new() { Area = 10, Animais = 20, PesoCompra = 450, PrecoCompra = 50, Dias = 180, PastoPronto = true };

    [TestMethod]
    public void PriorizaPrejuizoSuperlotacaoESanidade()
    {
        var resultado = Calculadora.Calcular(Entrada, new());
        CollectionAssert.AreEqual(new[] { "PREJUIZO", "SUPERLOTACAO", "SANIDADE", "MARGEM" }, resultado.Alertas.Select(a => a.Codigo).ToArray());
    }

    [TestMethod]
    public void SanidadePrecedeSubutilizacao()
    {
        var resultado = Calculadora.Calcular(Entrada with { Animais = 2, PrecoCompra = 1 }, new());
        CollectionAssert.AreEqual(new[] { "SANIDADE", "SUBUTILIZACAO" }, resultado.Alertas.Select(a => a.Codigo).ToArray());
    }

    [TestMethod]
    public void SemFatoresDeAlertaRetornaListaVazia()
    {
        var resultado = Calculadora.Calcular(Entrada with { Animais = 10, PrecoCompra = 1, Carbunculo = true, Carrapato = true, Vermifugo = true }, new());
        Assert.IsEmpty(resultado.Alertas);
    }
}
