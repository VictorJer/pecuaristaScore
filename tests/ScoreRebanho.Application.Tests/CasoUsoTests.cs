using ScoreRebanho.Domain;
namespace ScoreRebanho.Application.Tests;
[TestClass]
public class CasoUsoTests
{
 private sealed class Provedor : IProvedorPremissas { public Premissas Obter()=>new(); }
 private static EntradaScoreDto Base => new() { Area=10, Animais=10, PesoCompra=450,PrecoCompra=5,Dias=180,PastoPronto=true };
 [TestMethod] public void RetornaErrosPorCampo() { var r=new CalcularScoreRebanho(new Provedor()).Executar(new()); Assert.IsTrue(r.IsFailed); Assert.IsTrue(r.Errors.Any(x=>Equals(x.Metadata["Campo"],"Area"))); }
 [TestMethod] public void RetornaVersaoEValoresDeterministicos() {
  var s=new CalcularScoreRebanho(new Provedor()); var a=s.Executar(Base); var b=s.Executar(Base);
  Assert.IsTrue(a.IsSuccess); Assert.AreEqual("1.0",a.Value.VersaoPremissas); Assert.AreEqual(a.Value.Score,b.Value.Score);
  CollectionAssert.AreEqual(a.Value.Indicadores,b.Value.Indicadores);
 }
 [TestMethod] public void OverflowViraFalhaEsperada() {
  var r=new CalcularScoreRebanho(new Provedor()).Executar(Base with { PesoCompra=decimal.MaxValue });
  Assert.IsTrue(r.IsFailed); Assert.AreEqual("LIMITE_NUMERICO",r.Errors[0].Metadata["Codigo"]);
 }
}
