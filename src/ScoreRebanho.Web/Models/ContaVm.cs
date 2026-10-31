using System.ComponentModel.DataAnnotations;
namespace ScoreRebanho.Web.Models;

public class ContaVm
{
    [Required(ErrorMessage = "Informe seu e-mail."), EmailAddress(ErrorMessage = "E-mail inválido.")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Informe sua senha."), DataType(DataType.Password), Display(Name = "Senha")] public string Senha { get; set; } = "";
}
public class SenhaVm
{
    [Required, DataType(DataType.Password), Display(Name = "Senha atual")] public string Atual { get; set; } = "";
    [Required, DataType(DataType.Password), Display(Name = "Nova senha")] public string Nova { get; set; } = "";
}

