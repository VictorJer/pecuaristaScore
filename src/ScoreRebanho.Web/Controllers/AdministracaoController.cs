using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application;
using ScoreRebanho.Web.Models;
namespace ScoreRebanho.Web.Controllers;
[Authorize(Roles = "Administrador")]
public class AdministracaoController(ConsultarScores consulta, IMapper mapper, UserManager<IdentityUser> usuarios) : Controller
{
    public async Task<IActionResult> Index(int pagina = 1)
    {
        pagina = Math.Clamp(pagina, 1, 1000000);
        var resultado = await consulta.ListarAsync(pagina);
        var linhas = new List<ScoreResumoVm>();
        foreach (var score in resultado.Value)
        {
            var usuario = await usuarios.FindByIdAsync(score.UsuarioId);
            linhas.Add(new(score.Id, usuario?.Email ?? "Conta indisponível", score.CriadoEmUtc, score.Resultado.Score, score.Resultado.Classificacao));
        }
        return View(new HistoricoVm(pagina, linhas));
    }
    public async Task<IActionResult> Detalhes(Guid id)
    {
        var resultado = await consulta.ObterAsync(id);
        if (resultado.IsFailed) return NotFound();
        var vm = mapper.Map<ResultadoVm>(resultado.Value.Resultado);
        vm.Entrada = mapper.Map<SimulacaoVm>(resultado.Value.Entrada);
        ViewData["SomenteLeitura"] = true;
        return View("~/Views/Simulacao/Relatorio.cshtml", vm);
    }
}
