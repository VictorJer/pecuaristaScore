namespace ScoreRebanho.Domain;
public enum Posse { Proprio, Arrendado }
public enum Pastagem { Verao, Inverno }
public record Entrada
{
 public decimal Area { get; init; }
 public Posse Posse { get; init; }
 public decimal? ArrendamentoMensal { get; init; }
 public Pastagem Pastagem { get; init; }
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
 public IEnumerable<(string Campo, string Mensagem)> Validar()
 {
  if (Area <= 0) yield return (nameof(Area), "Informe área maior que zero.");
  if (Animais <= 0) yield return (nameof(Animais), "Informe quantidade positiva.");
  if (PesoCompra <= 0) yield return (nameof(PesoCompra), "Informe peso maior que zero.");
  if (PrecoCompra <= 0) yield return (nameof(PrecoCompra), "Informe preço maior que zero.");
  if (Dias <= 0) yield return (nameof(Dias), "Informe período positivo.");
  if (!Enum.IsDefined(Posse)) yield return (nameof(Posse), "Posse inválida.");
  if (!Enum.IsDefined(Pastagem)) yield return (nameof(Pastagem), "Pastagem inválida.");
  if (Posse == Posse.Arrendado && ArrendamentoMensal is null or < 0) yield return (nameof(ArrendamentoMensal), "Informe arrendamento não negativo.");
  if (Rotacao && Piquetes is null or <= 0) yield return (nameof(Piquetes), "Informe piquetes positivos.");
 }
}
public record Premissas
{
 public string Versao { get; init; } = "1.0";
 public decimal KgArroba { get; init; } = 30;
 public decimal KgUa { get; init; } = 450;
 public decimal PrecoArroba { get; init; } = 340;
 public decimal CapacidadeVerao { get; init; } = 1;
 public decimal CapacidadeInverno { get; init; } = 1.2m;
 public decimal GmdBase { get; init; } = .55m;
 public decimal GmdRaca { get; init; } = .10m;
 public decimal GmdCarrapato { get; init; } = .04m;
 public decimal GmdVermifugo { get; init; } = .05m;
 public decimal GmdModificador { get; init; } = .12m;
 public decimal ManutencaoVerao { get; init; } = 300;
 public decimal ManutencaoInverno { get; init; } = 450;
 public decimal FormacaoVerao { get; init; } = 2385;
 public decimal FormacaoInverno { get; init; } = 2400;
 public decimal SuplementoDia { get; init; } = 3;
 public decimal RotacaoBase { get; init; } = 150;
 public decimal RotacaoPiquete { get; init; } = 60;
 public decimal DoseCarbunculo { get; init; } = 2;
 public decimal FrascoCarbunculo { get; init; } = 100;
 public decimal PrecoCarbunculo { get; init; } = 40;
 public decimal DoseCarrapato { get; init; } = 4;
 public decimal FrascoCarrapato { get; init; } = 500;
 public decimal PrecoCarrapato { get; init; } = 100;
 public decimal DoseVermifugo { get; init; } = 6;
 public decimal FrascoVermifugo { get; init; } = 500;
 public decimal PrecoVermifugo { get; init; } = 85;
 public decimal DoseModificador { get; init; } = .05m;
 public decimal SacoModificador { get; init; } = 25;
 public decimal PrecoModificador { get; init; } = 120;
 public void Validar()
 {
  if (string.IsNullOrWhiteSpace(Versao) || KgArroba <= 0 || KgUa <= 0 || CapacidadeVerao <= 0 || CapacidadeInverno <= 0 || FrascoCarbunculo <= 0 || FrascoCarrapato <= 0 || FrascoVermifugo <= 0 || SacoModificador <= 0 || GetType().GetProperties().Where(p => p.PropertyType == typeof(decimal)).Any(p => (decimal)p.GetValue(this)! < 0)) throw new InvalidOperationException("Premissas inválidas.");
 }
}
public record Item(string Nome, decimal Valor, string Unidade, string Explicacao);
public record Alerta(string Codigo, string Mensagem);
public record Resultado(int Score, string Classificacao, IReadOnlyList<Item> Componentes, IReadOnlyList<Item> Indicadores, IReadOnlyList<Item> Custos, IReadOnlyList<Item> Capital, IReadOnlyList<Alerta> Alertas, Premissas Premissas);

