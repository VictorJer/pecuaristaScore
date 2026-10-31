using System.Security.Claims;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application;
using ScoreRebanho.Web.Models;

namespace ScoreRebanho.Web.Controllers;

[Authorize]
public sealed class RebanhosController(GerenciarRebanhos servico, IMapper mapper) : Controller
{
    private string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1)
    {
        var resultado = await servico.ListarAsync(UsuarioId, pagina);
        if (resultado.IsFailed) return Challenge();
        return View(mapper.Map<PaginaRebanhosVm>(resultado.Value));
    }
    [HttpGet] public IActionResult Novo() => View("Formulario", new RebanhoFormularioVm());
    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var resultado = await servico.ObterAsync(UsuarioId, id);
        return resultado.IsFailed ? NotFound() : View("Formulario", mapper.Map<RebanhoFormularioVm>(resultado.Value));
    }
    [HttpPost]
    public async Task<IActionResult> Salvar(RebanhoFormularioVm vm)
    {
        if (!ModelState.IsValid) return View("Formulario", vm);
        var resultado = await servico.SalvarAsync(UsuarioId, mapper.Map<SalvarRebanhoDto>(vm));
        if (resultado.IsFailed)
        {
            if (NaoEncontrado(resultado.Errors)) return NotFound();
            AdicionarErros(resultado.Errors);
            return View("Formulario", vm);
        }
        TempData["Mensagem"] = vm.Id.HasValue ? "Rebanho atualizado e score recalculado." : "Rebanho cadastrado.";
        return RedirectToAction(nameof(Detalhes), new { id = resultado.Value.Id });
    }
    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id)
    {
        var resultado = await servico.ObterAsync(UsuarioId, id);
        return resultado.IsFailed ? NotFound() : View(mapper.Map<RebanhoVm>(resultado.Value));
    }
    [HttpGet]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var resultado = await servico.ObterAsync(UsuarioId, id);
        return resultado.IsFailed ? NotFound() : View(mapper.Map<RebanhoVm>(resultado.Value));
    }
    [HttpPost]
    public async Task<IActionResult> ConfirmarExclusao(ExcluirRebanhoVm vm)
    {
        if (!ModelState.IsValid) return BadRequest();
        var resultado = await servico.ExcluirAsync(UsuarioId, vm.Id, vm.Versao);
        if (resultado.IsFailed)
        {
            if (NaoEncontrado(resultado.Errors)) return NotFound();
            var atual = await servico.ObterAsync(UsuarioId, vm.Id);
            if (atual.IsFailed) return NotFound();
            AdicionarErros(resultado.Errors);
            // A nova versão exige outra confirmação explícita.
            ModelState.Remove(nameof(vm.Versao));
            return View("Excluir", mapper.Map<RebanhoVm>(atual.Value));
        }
        TempData["Mensagem"] = "Rebanho excluído. Os scores anteriores permanecem no histórico administrativo.";
        return RedirectToAction(nameof(Index));
    }
    private static bool NaoEncontrado(IEnumerable<IError> erros) => erros.Any(e => e.Metadata.TryGetValue("Codigo", out var codigo) && Equals(codigo, "NAO_ENCONTRADO"));
    private void AdicionarErros(IEnumerable<IError> erros)
    {
        foreach (var erro in erros)
            ModelState.AddModelError(erro.Metadata.TryGetValue("Campo", out var campo) ? campo.ToString()! : "", erro.Message);
    }
}
