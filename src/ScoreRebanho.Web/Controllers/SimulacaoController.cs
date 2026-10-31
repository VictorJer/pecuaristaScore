using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application;
using ScoreRebanho.Web.Models;
namespace ScoreRebanho.Web.Controllers;

[Authorize]
public class SimulacaoController(CalcularScoreRebanho servico, IMapper mapper) : Controller
{
    [HttpGet] public IActionResult Index() => View(new SimulacaoVm());
    [HttpPost]
    public IActionResult Gerar(SimulacaoVm vm)
    {
        if (!ModelState.IsValid) return View("Index", vm);
        var result = servico.Executar(mapper.Map<EntradaScoreDto>(vm));
        if (result.IsFailed)
        {
            foreach (var erro in result.Errors) ModelState.AddModelError(erro.Metadata.TryGetValue("Campo", out var campo) ? campo.ToString()! : "", erro.Message);
            return View("Index", vm);
        }
        var saida = mapper.Map<ResultadoVm>(result.Value); saida.Entrada = vm;
        return View("Relatorio", saida);
    }
    [HttpPost] public IActionResult Editar(SimulacaoVm vm) => View("Index", vm);
}
