using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ScoreRebanho.Application;
namespace ScoreRebanho.Web.Models;
public class SimulacaoVm
{
 [Display(Name="Área (ha)")] public decimal Area { get; set; }
 [Display(Name="Tipo de posse")] public int Posse { get; set; }
 [Display(Name="Arrendamento mensal (R$)")] public decimal? ArrendamentoMensal { get; set; }
 [Display(Name="Pastagem")] public int Pastagem { get; set; }
 [Display(Name="Pasto pronto")] public bool PastoPronto { get; set; }
 [Display(Name="Rotação de pastagem")] public bool Rotacao { get; set; }
 [Display(Name="Número de piquetes")] public int? Piquetes { get; set; }
 [Display(Name="Quantidade de animais")] public int Animais { get; set; }
 [Display(Name="Peso médio de compra (kg)")] public decimal PesoCompra { get; set; }
 [Display(Name="Preço de compra (R$/kg vivo)")] public decimal PrecoCompra { get; set; }
 [Display(Name="Dias na pastagem")] public int Dias { get; set; }
 [Display(Name="Raça pura para corte")] public bool RacaPura { get; set; }
 [Display(Name="Vacina de carbúnculo")] public bool Carbunculo { get; set; }
 [Display(Name="Controle de carrapato")] public bool Carrapato { get; set; }
 [Display(Name="Vermífugo")] public bool Vermifugo { get; set; }
 [Display(Name="Modificador orgânico")] public bool Modificador { get; set; }
}
public record ItemVm(string Nome, decimal Valor, string Unidade, string Explicacao);
public record AlertaVm(string Codigo, string Mensagem);
public class ResultadoVm
{
 public int Score { get; set; }
 public string Classificacao { get; set; } = "";
 public ItemVm[] Componentes { get; set; } = [];
 public ItemVm[] Indicadores { get; set; } = [];
 public ItemVm[] Custos { get; set; } = [];
 public ItemVm[] Capital { get; set; } = [];
 public AlertaVm[] Alertas { get; set; } = [];
 public string VersaoPremissas { get; set; } = "";
 public IReadOnlyDictionary<string,string> Premissas { get; set; } = new Dictionary<string,string>();
 public SimulacaoVm Entrada { get; set; } = new();
}
public sealed class SimulacaoProfile : Profile
{
 public SimulacaoProfile()
 {
  CreateMap<SimulacaoVm,EntradaScoreDto>();
  CreateMap<ItemDto,ItemVm>(); CreateMap<AlertaDto,AlertaVm>();
  CreateMap<ResultadoScoreDto,ResultadoVm>().ForMember(x => x.Entrada, o => o.Ignore());
 }
}
public class ContaVm
{
 [Required(ErrorMessage="Informe seu e-mail."), EmailAddress(ErrorMessage="E-mail inválido.")] public string Email { get; set; } = "";
 [Required(ErrorMessage="Informe sua senha."), DataType(DataType.Password), Display(Name="Senha")] public string Senha { get; set; } = "";
}
public class SenhaVm
{
 [Required, DataType(DataType.Password), Display(Name="Senha atual")] public string Atual { get; set; } = "";
 [Required, DataType(DataType.Password), Display(Name="Nova senha")] public string Nova { get; set; } = "";
}
