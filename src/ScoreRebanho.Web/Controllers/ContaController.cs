using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Web.Models;
namespace ScoreRebanho.Web.Controllers;
public class ContaController(UserManager<IdentityUser> users, SignInManager<IdentityUser> login) : Controller
{
 [HttpGet] public IActionResult Entrar() => View(new ContaVm());
 [HttpPost] public async Task<IActionResult> Entrar(ContaVm vm)
 {
  if (!ModelState.IsValid) return View(vm);
  var r = await login.PasswordSignInAsync(vm.Email, vm.Senha, false, true);
  if (r.Succeeded) return RedirectToAction("Index", "Simulacao");
  ModelState.AddModelError("", r.IsLockedOut ? "Muitas tentativas. Tente novamente mais tarde." : "E-mail ou senha inválidos.");
  return View(vm);
 }
 [HttpGet] public IActionResult Cadastrar() => View(new ContaVm());
 [HttpPost] public async Task<IActionResult> Cadastrar(ContaVm vm)
 {
  if (!ModelState.IsValid) return View(vm);
  var user = new IdentityUser { UserName=vm.Email, Email=vm.Email };
  var r = await users.CreateAsync(user, vm.Senha);
  if (r.Succeeded) { await login.SignInAsync(user,false); return RedirectToAction("Index","Simulacao"); }
  foreach (var e in r.Errors) ModelState.AddModelError("", Traduzir(e));
  return View(vm);
 }
 [Authorize, HttpPost] public async Task<IActionResult> Sair() { await login.SignOutAsync(); return RedirectToAction(nameof(Entrar)); }
 [Authorize, HttpGet] public IActionResult Senha() => View(new SenhaVm());
 [Authorize, HttpPost] public async Task<IActionResult> Senha(SenhaVm vm)
 {
  if (!ModelState.IsValid) return View(vm);
  var user = await users.GetUserAsync(User);
  if (user is null) return Challenge();
  var r = await users.ChangePasswordAsync(user,vm.Atual,vm.Nova);
  if (r.Succeeded) { await login.RefreshSignInAsync(user); TempData["Mensagem"]="Senha alterada."; return RedirectToAction(nameof(Senha)); }
  foreach (var e in r.Errors) ModelState.AddModelError("", Traduzir(e));
  return View(vm);
 }
 private static string Traduzir(IdentityError e) => e.Code switch
 {
  "DuplicateUserName" or "DuplicateEmail" => "Não foi possível cadastrar este e-mail.",
  "PasswordMismatch" => "Senha atual incorreta.",
  "InvalidEmail" or "InvalidUserName" => "Informe um e-mail válido.",
  _ => "A senha deve conter ao menos 6 caracteres, maiúscula, minúscula, número e símbolo."
 };
}
