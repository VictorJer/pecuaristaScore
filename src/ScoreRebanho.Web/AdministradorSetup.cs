using Microsoft.AspNetCore.Identity;
namespace ScoreRebanho.Web;
public static class AdministradorSetup
{
    public static async Task PromoverAsync(IServiceProvider services, string email)
    {
        using var scope = services.CreateScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var papeis = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var usuario = await usuarios.FindByEmailAsync(email)
            ?? throw new InvalidOperationException("Cadastre a conta antes de atribuir o papel de administrador.");
        if (!await papeis.RoleExistsAsync("Administrador"))
        {
            var criacao = await papeis.CreateAsync(new IdentityRole("Administrador"));
            if (!criacao.Succeeded) throw new InvalidOperationException("Não foi possível criar o papel de administrador.");
        }
        var resultado = await usuarios.AddToRoleAsync(usuario, "Administrador");
        if (!resultado.Succeeded) throw new InvalidOperationException("Não foi possível atribuir o papel de administrador.");
    }
}
