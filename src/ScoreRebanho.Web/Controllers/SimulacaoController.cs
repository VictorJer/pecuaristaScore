using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application;
using ScoreRebanho.Web.Models;
namespace ScoreRebanho.Web.Controllers;

[Authorize]
public class SimulacaoController(GerarScore servico, IMapper mapper, CalcularScoreRebanho calculadora) : Controller
{
    [HttpGet] public IActionResult Index() => View(new SimulacaoVm());
    [HttpPost]
    public IActionResult Resumo(SimulacaoVm vm)
    {
        if (!ModelState.IsValid) return View("Index", vm);
        var resultado = calculadora.Resumir(mapper.Map<EntradaScoreDto>(vm));
        if (resultado.IsFailed)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(erro.Metadata.TryGetValue("Campo", out var campo) ? campo.ToString()! : "", erro.Message);
        }
        else ViewData["Resumo"] = mapper.Map<ResumoVm>(resultado.Value);
        return View("Index", vm);
    }
    [HttpPost]
    public async Task<IActionResult> Gerar(SimulacaoVm vm)
    {
        if (!ModelState.IsValid) return View("Index", vm);
        var result = await servico.ExecutarAsync(mapper.Map<EntradaScoreDto>(vm), User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
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
