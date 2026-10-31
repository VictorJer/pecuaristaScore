using ScoreRebanho.Domain;
namespace ScoreRebanho.Domain.Tests;
[TestClass]
public class CalculoTests
{
 private static Entrada Base => new() { Area=10, Animais=10, PesoCompra=450, PrecoCompra=5, Dias=180, PastoPronto=true, Carbunculo=true, Carrapato=true, Vermifugo=true, RacaPura=true };
 private static decimal Valor(Resultado r,string nome) => r.Indicadores.Single(x=>x.Nome==nome).Valor;
 [TestMethod]
 public void LotacaoRespeitaFaixas()
 {
  Assert.AreEqual(18m,Calculadora.PontosLotacao(0)); Assert.AreEqual(24m,Calculadora.PontosLotacao(.25m));
  Assert.AreEqual(30m,Calculadora.PontosLotacao(.5m)); Assert.AreEqual(30m,Calculadora.PontosLotacao(1));
  Assert.AreEqual(15m,Calculadora.PontosLotacao(1.4m)); Assert.AreEqual(0m,Calculadora.PontosLotacao(1.8m)); Assert.AreEqual(0m,Calculadora.PontosLotacao(2));
 }
 [TestMethod]
 public void CenarioNumericoIndependente()
 {
  var r=Calculadora.Calcular(Base,new());
  Assert.AreEqual(.74m,Valor(r,"GMD")); Assert.AreEqual(583.2m,Valor(r,"Peso de venda por animal"));
  Assert.AreEqual(9.85m,Valor(r,"Animais vendidos estimados")); Assert.AreEqual(5744.52m,Valor(r,"Peso total vendido"));
  Assert.AreEqual(65104.56m,Valor(r,"Receita"));
  Assert.AreEqual(22725m+3000m*180/365,Valor(r,"Gasto total"));
  Assert.AreEqual(r.Custos.Sum(x=>x.Valor),Valor(r,"Gasto total")); Assert.AreEqual(r.Capital.Sum(x=>x.Valor),Valor(r,"Capital inicial"));
  Assert.AreEqual(Valor(r,"Receita")*.9m,Valor(r,"Receita com preço −10%"));
 }
 [TestMethod]
 public void MortalidadeETetoGmd()
 {
  var r=Calculadora.Calcular(Base with { Area=.1m },new());
  Assert.AreEqual(15m,Valor(r,"Mortalidade")); Assert.AreEqual(.74m*.45m,Valor(r,"GMD"));
  Assert.AreEqual(22500m,r.Custos.Single(x=>x.Nome=="Compra do gado").Valor);
 }
 [TestMethod]
 public void RotacaoTemLimiarETeto()
 {
  Assert.AreEqual(10m,Valor(Calculadora.Calcular(Base with { Rotacao=true,Piquetes=2 },new()),"Capacidade utilizada"));
  Assert.AreEqual(13m,Valor(Calculadora.Calcular(Base with { Rotacao=true,Piquetes=3 },new()),"Capacidade utilizada"));
  Assert.AreEqual(16m,Valor(Calculadora.Calcular(Base with { Rotacao=true,Piquetes=20 },new()),"Capacidade utilizada"));
 }
 [TestMethod]
 public void DosesIncluemDiaInicialEDiaFinal()
 {
  foreach(var intervalo in new[]{90,150}) {
   Assert.AreEqual(1,Calculadora.Aplicacoes(intervalo-1,intervalo));
   Assert.AreEqual(2,Calculadora.Aplicacoes(intervalo,intervalo));
   Assert.AreEqual(2,Calculadora.Aplicacoes(intervalo+1,intervalo));
  }
 }
 [TestMethod]
 public void EmbalagensArredondadasParaCima()
 {
  var a=Calculadora.Calcular(Base with { Animais=50 },new()); var b=Calculadora.Calcular(Base with { Animais=51 },new());
  Assert.AreEqual(40m,a.Custos.Single(x=>x.Nome=="Carbúnculo").Valor); Assert.AreEqual(80m,b.Custos.Single(x=>x.Nome=="Carbúnculo").Valor);
 }
 [TestMethod]
 public void CapitalDistingueInvestimentoDeCusto()
 {
  var r=Calculadora.Calcular(Base with { PastoPronto=false,Rotacao=true,Piquetes=3 },new());
  Assert.AreEqual(23850m,r.Capital.Single(x=>x.Nome=="Formação integral").Valor);
  Assert.AreEqual(4770m,r.Custos.Single(x=>x.Nome=="Formação do pasto").Valor);
  Assert.AreEqual(3300m*180/3650,r.Custos.Single(x=>x.Nome=="Rotação amortizada").Valor);
 }
 [TestMethod]
 public void SuplementacaoProporcional()
 {
  var r=Calculadora.Calcular(Base with { Animais=12 },new());
  Assert.AreEqual(648m,r.Custos.Single(x=>x.Nome=="Suplementação").Valor);
  Assert.AreEqual(0m,Calculadora.Calcular(Base with { Animais=11 },new()).Custos.Single(x=>x.Nome=="Suplementação").Valor);
 }
 [TestMethod]
 public void FinanceiroEClassificacao()
 {
  Assert.AreEqual(0m,Calculadora.PontosFinanceiros(-.1m)); Assert.AreEqual(15m,Calculadora.PontosFinanceiros(.05m)); Assert.AreEqual(30m,Calculadora.PontosFinanceiros(.2m));
  foreach(var limite in new[]{40,60,80}) Assert.AreEqual(limite,Calculadora.ArredondarScore(limite-.5m));
  Assert.AreEqual("Risco baixo",Calculadora.Classificar(80)); Assert.AreEqual("Risco moderado",Calculadora.Classificar(79)); Assert.AreEqual("Risco alto",Calculadora.Classificar(59)); Assert.AreEqual("Risco muito alto",Calculadora.Classificar(39));
 }
 [TestMethod]
 public void ValidacaoCondicional()
 {
  Assert.IsTrue((Base with { Posse=Posse.Arrendado }).Validar().Any());
  Assert.IsTrue((Base with { Rotacao=true }).Validar().Any());
  Assert.IsFalse((Base with { Posse=Posse.Arrendado,ArrendamentoMensal=0,Rotacao=true,Piquetes=1 }).Validar().Any());
  Assert.IsTrue((Base with { PrecoCompra=0 }).Validar().Any());
 }
}
