using FluentResults;
using ScoreRebanho.Domain;
namespace ScoreRebanho.Application;
public record EntradaScoreDto
{
 public decimal Area { get; init; }
 public int Posse { get; init; }
 public decimal? ArrendamentoMensal { get; init; }
 public int Pastagem { get; init; }
 public bool PastoPronto { get; init; }
 public bool Rotacao { get; init; }
 public int? Piquetes { get; init; }
 public int Animais { get; init; }
 public decimal PesoCompra { get; init; }
 public decimal PrecoCompra { get; init; }
 public int Dias { get; init; }
 public bool RacaPura { get; init; }
 public bool Carbunculo { get; init; }
 public bool Carrapato { get; init; }
 public bool Vermifugo { get; init; }
 public bool Modificador { get; init; }
}
public record ItemDto(string Nome, decimal Valor, string Unidade, string Explicacao);
public record AlertaDto(string Codigo, string Mensagem);
public record ResultadoScoreDto(int Score, string Classificacao, ItemDto[] Componentes, ItemDto[] Indicadores, ItemDto[] Custos, ItemDto[] Capital, AlertaDto[] Alertas, string VersaoPremissas, IReadOnlyDictionary<string, string> Premissas);
public interface IProvedorPremissas { Premissas Obter(); }
public sealed class CalcularScoreRebanho(IProvedorPremissas provedor)
{
 public Result<ResultadoScoreDto> Executar(EntradaScoreDto d)
 {
  var e = new Entrada { Area=d.Area, Posse=(Posse)d.Posse, ArrendamentoMensal=d.ArrendamentoMensal, Pastagem=(Pastagem)d.Pastagem, PastoPronto=d.PastoPronto, Rotacao=d.Rotacao, Piquetes=d.Piquetes, Animais=d.Animais, PesoCompra=d.PesoCompra, PrecoCompra=d.PrecoCompra, Dias=d.Dias, RacaPura=d.RacaPura, Carbunculo=d.Carbunculo, Carrapato=d.Carrapato, Vermifugo=d.Vermifugo, Modificador=d.Modificador };
  var erros = e.Validar().Select(x => new Error(x.Mensagem).WithMetadata("Campo", x.Campo).WithMetadata("Codigo", "ENTRADA_INVALIDA")).ToArray();
  if (erros.Length > 0) return Result.Fail<ResultadoScoreDto>(erros);
  try
  {
   var r = Calculadora.Calcular(e, provedor.Obter());
   static ItemDto[] Map(IEnumerable<Item> itens) => itens.Select(i => new ItemDto(i.Nome,i.Valor,i.Unidade,i.Explicacao)).ToArray();
   var premissas = typeof(Premissas).GetProperties().ToDictionary(p => p.Name, p => Convert.ToString(p.GetValue(r.Premissas), System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) ?? "");
   return Result.Ok(new ResultadoScoreDto(r.Score, r.Classificacao, Map(r.Componentes), Map(r.Indicadores), Map(r.Custos), Map(r.Capital), r.Alertas.Select(a => new AlertaDto(a.Codigo,a.Mensagem)).ToArray(), r.Premissas.Versao, premissas));
  }
  catch (DivideByZeroException) { return Result.Fail<ResultadoScoreDto>(new Error("Não foi possível calcular com estes valores: capacidade inferior à precisão numérica suportada.").WithMetadata("Codigo", "CALCULO_INDISPONIVEL")); }
  catch (OverflowException) { return Result.Fail<ResultadoScoreDto>(new Error("Valores excedem a capacidade de cálculo. Reduza os valores informados.").WithMetadata("Codigo", "LIMITE_NUMERICO")); }
 }
}

