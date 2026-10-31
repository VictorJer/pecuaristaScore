using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScoreRebanho.Application;
using ScoreRebanho.Infrastructure;
using ScoreRebanho.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AdicionarInfrastructure(builder.Configuration);
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Stores.MaxLengthForKeys = 128;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AdicionarStores()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Entrar";
    options.AccessDeniedPath = "/Conta/AcessoNegado";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddScoped<CalcularScoreRebanho>();
builder.Services.AddScoped<GerarScore>();
builder.Services.AddScoped<GerenciarRebanhos>();
builder.Services.AddScoped<ConsultarScores>();
builder.Services.AddAutoMapper(options =>
    options.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"], typeof(SimulacaoProfile));
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        campo => $"Informe um número válido em {campo}.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (valor, campo) => $"Valor inválido em {campo}.");
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Informe um valor.");
});

var app = builder.Build();
if (args.Contains("--migrate"))
{
    await app.Services.MigrarBancoAsync();
    return;
}

if (args.Contains("--administrador"))
{
    var email = builder.Configuration["AdministradorEmail"];
    if (string.IsNullOrWhiteSpace(email)) throw new InvalidOperationException("Informe AdministradorEmail.");
    await ScoreRebanho.Web.AdministradorSetup.PromoverAsync(app.Services, email);
    return;
}
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("pt-BR")
    .AddSupportedCultures("pt-BR")
    .AddSupportedUICultures("pt-BR"));
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Rebanhos}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();

public partial class Program { }




