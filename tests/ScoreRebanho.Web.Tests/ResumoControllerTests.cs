using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using ScoreRebanho.Application;
using ScoreRebanho.Domain;
using ScoreRebanho.Web.Controllers;
using ScoreRebanho.Web.Models;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public class ResumoControllerTests
{
    private sealed class Provedor : IProvedorPremissas { public Premissas Obter() => new(); }
    private sealed class HistoricoProibido : IHistoricoScores
    {
        public Task SalvarAsync(ScoreSalvoDto score) => throw new AssertFailedException("Resumo não deve persistir.");
        public Task<IReadOnlyList<ScoreSalvoDto>> ListarAsync(int pagina) => throw new AssertFailedException();
        public Task<ScoreSalvoDto?> ObterAsync(Guid id) => throw new AssertFailedException();
    }
    private static SimulacaoController Criar()
    {
        var calculadora = new CalcularScoreRebanho(new Provedor());
        var mapper = new MapperConfiguration(c => c.AddProfile<SimulacaoProfile>(), NullLoggerFactory.Instance).CreateMapper();
        return new(new GerarScore(calculadora, new HistoricoProibido()), mapper, calculadora);
    }
    [TestMethod]
    public void ResumoPreservaFormularioSemPersistirNemExibirRelatorio()
    {
        var controller = Criar();
        var entrada = new SimulacaoVm { Area = 10, Animais = 10, PesoCompra = 450, PrecoCompra = 5, Dias = 180 };
        var resposta = (ViewResult)controller.Resumo(entrada);
        Assert.AreEqual("Index", resposta.ViewName);
        Assert.AreSame(entrada, resposta.Model);
        var resumo = (ResumoVm)resposta.ViewData["Resumo"]!;
        Assert.AreEqual(100m, resumo.Ocupacao);
    }
    [TestMethod]
    public void ResumoInvalidoRetornaErrosPorCampoSemDadosParciais()
    {
        var controller = Criar();
        var resposta = (ViewResult)controller.Resumo(new());
        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.IsTrue(controller.ModelState.ContainsKey("Area"));
        Assert.IsNull(resposta.ViewData["Resumo"]);
    }
    [TestMethod]
    public void ErroDeBindingPreservaEntradaSemCalcular()
    {
        var controller = Criar();
        controller.ModelState.AddModelError("Area", "Informe um número válido.");
        var entrada = new SimulacaoVm { Animais = 12 };
        var resposta = (ViewResult)controller.Resumo(entrada);
        Assert.AreSame(entrada, resposta.Model);
        Assert.IsNull(resposta.ViewData["Resumo"]);
    }
}
