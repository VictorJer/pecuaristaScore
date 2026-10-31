using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application;
using ScoreRebanho.Infrastructure;
using ScoreRebanho.Web.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AdicionarInfrastructure(builder.Configuration);
builder.Services.AddIdentity<IdentityUser,IdentityRole>(o => { o.Stores.MaxLengthForKeys=128; o.User.RequireUniqueEmail=true; o.SignIn.RequireConfirmedAccount=false; }).AdicionarStores().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath="/Conta/Entrar"; o.Cookie.HttpOnly=true; o.Cookie.SecurePolicy=builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always; });
builder.Services.AddScoped<CalcularScoreRebanho>();
builder.Services.AddAutoMapper(c => { c.LicenseKey=builder.Configuration["AutoMapper:LicenseKey"]; }, typeof(SimulacaoProfile));
builder.Services.AddControllersWithViews(o => { o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); o.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(campo => $"Informe um número válido em {campo}."); o.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((valor,campo) => $"Valor inválido em {campo}."); o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Informe um valor."); });
var app = builder.Build();
if (args.Contains("--migrate")) { await app.Services.MigrarBancoAsync(); return; }
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("pt-BR").AddSupportedCultures("pt-BR").AddSupportedUICultures("pt-BR"));
app.UseHttpsRedirection(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Simulacao}/{action=Index}/{id?}").WithStaticAssets();
app.Run();
public partial class Program { }



