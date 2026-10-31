using System.ComponentModel.DataAnnotations;


namespace ScoreRebanho.Web.Models;

public class SimulacaoVm
{
    [Display(Name = "Área (ha)")] public decimal Area { get; set; }
    [Display(Name = "Tipo de posse")] public int Posse { get; set; }
    [Display(Name = "Arrendamento mensal (R$)")] public decimal? ArrendamentoMensal { get; set; }
    [Display(Name = "Pastagem")] public int Pastagem { get; set; }
    [Display(Name = "Pasto pronto")] public bool PastoPronto { get; set; }
    [Display(Name = "Rotação de pastagem")] public bool Rotacao { get; set; }
    [Display(Name = "Número de piquetes")] public int? Piquetes { get; set; }
    [Display(Name = "Quantidade de animais")] public int Animais { get; set; }
    [Display(Name = "Peso médio de compra (kg)")] public decimal PesoCompra { get; set; }
    [Display(Name = "Preço de compra (R$/kg vivo)")] public decimal PrecoCompra { get; set; }
    [Display(Name = "Dias na pastagem")] public int Dias { get; set; }
    [Display(Name = "Raça pura para corte")] public bool RacaPura { get; set; }
    [Display(Name = "Vacina de carbúnculo")] public bool Carbunculo { get; set; }
    [Display(Name = "Controle de carrapato")] public bool Carrapato { get; set; }
    [Display(Name = "Vermífugo")] public bool Vermifugo { get; set; }
    [Display(Name = "Modificador orgânico")] public bool Modificador { get; set; }
}

