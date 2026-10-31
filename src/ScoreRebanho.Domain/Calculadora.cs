namespace ScoreRebanho.Domain;
public static class Calculadora
{
 public static decimal PontosLotacao(decimal o) => o < .5m ? 18 + 24 * o : o <= 1 ? 30 : Math.Max(0, 30 * (1.8m - o) / .8m);
 public static decimal PontosFinanceiros(decimal margem) => Math.Clamp((margem + .1m) * 100, 0, 30);
 public static int Aplicacoes(int dias, int intervalo) => checked(1 + dias / intervalo);
 public static int ArredondarScore(decimal score) => (int)Math.Round(Math.Clamp(score, 0, 100), 0, MidpointRounding.AwayFromZero);
 public static string Classificar(int score) => score >= 80 ? "Risco baixo" : score >= 60 ? "Risco moderado" : score >= 40 ? "Risco alto" : "Risco muito alto";
 public static Resultado Calcular(Entrada e, Premissas p)
 {
  if (e.Validar().Any()) throw new ArgumentException("Entradas inválidas.");
  p.Validar();
  var verao = e.Pastagem == Pastagem.Verao;
  var efetiva = e.Rotacao && e.Piquetes >= 3;
  var baseUa = e.Area * (verao ? p.CapacidadeVerao : p.CapacidadeInverno);
  var capacidade = baseUa * (efetiva ? Math.Min(1.6m, 1 + .15m * (e.Piquetes!.Value - 1)) : 1);
  var ua = e.Animais * e.PesoCompra / p.KgUa;
  var o = ua / capacidade;
  var excesso = Math.Max(0, o - 1);
  var gmd = (p.GmdBase + (e.RacaPura ? p.GmdRaca : 0) + (e.Carrapato ? p.GmdCarrapato : 0) + (e.Vermifugo ? p.GmdVermifugo : 0) + (e.Modificador ? p.GmdModificador : 0)) * Math.Max(.45m, 1 - .5m * excesso);
  var mortalidade = Math.Min(.15m, .015m + (e.Carbunculo ? 0 : .015m) + (e.Carrapato ? 0 : .012m) + (e.Vermifugo ? 0 : .008m) + .04m * excesso);
  var peso = e.PesoCompra + gmd * e.Dias;
  var vendidos = e.Animais * (1 - mortalidade);
  var kg = vendidos * peso;
  var arrobas = kg / p.KgArroba;
  var custos = new List<Item>();
  var capital = new List<Item>();
  var indicadores = new List<Item>();
  void Custo(string n, decimal v, string x) => custos.Add(new(n, v, "R$", x));
  Custo("Compra do gado", e.Animais * e.PesoCompra * e.PrecoCompra, "Lote inicial × peso × preço/kg.");
  capital.Add(custos[^1]);
  Custo("Manutenção do pasto", e.Area * (verao ? p.ManutencaoVerao : p.ManutencaoInverno) * e.Dias / 365, "Custo anual × hectares × dias/365.");
  var formacao = e.PastoPronto ? 0 : e.Area * (verao ? p.FormacaoVerao : p.FormacaoInverno);
  Custo("Formação do pasto", formacao / (verao ? 5 : 1), "Verão: 1/5 do investimento; inverno: integral.");
  capital.Add(new("Formação integral", formacao, "R$", "Investimento quando o pasto não está pronto."));
  var rotacao = e.Rotacao ? e.Area * (p.RotacaoBase + p.RotacaoPiquete * e.Piquetes!.Value) : 0;
  Custo("Rotação amortizada", rotacao * e.Dias / 3650, "Investimento × dias/3650, mesmo com menos de três piquetes.");
  capital.Add(new("Rotação integral", rotacao, "R$", "Área × (custo base + custo por piquete × piquetes)."));
  Custo("Arrendamento", e.Posse == Posse.Arrendado ? e.ArrendamentoMensal!.Value * e.Dias / 30 : 0, "Mensalidade × dias/30.");
  Custo("Suplementação", e.Animais * (decimal)e.Dias * p.SuplementoDia * Math.Max(0, o - 1.10m), "Animais × dias × tarifa × máximo(0, ocupação decimal − 1,10).");
  void Produto(string nome, bool ativo, decimal dose, int aplicacoes, decimal embalagem, decimal preco, string unidade)
  {
   var volume = ativo ? e.Animais * dose * aplicacoes : 0;
   var frascos = Math.Ceiling(volume / embalagem);
   Custo(nome, frascos * preco, $"{(ativo ? aplicacoes : 0)} aplicações/dias; {volume} {unidade}; {frascos} embalagens de {embalagem} {unidade}.");
   capital.Add(custos[^1]);
  }
  Produto("Carbúnculo", e.Carbunculo, p.DoseCarbunculo, 1, p.FrascoCarbunculo, p.PrecoCarbunculo, "ml");
  Produto("Carrapato", e.Carrapato, p.DoseCarrapato, Aplicacoes(e.Dias, 90), p.FrascoCarrapato, p.PrecoCarrapato, "ml");
  Produto("Vermífugo", e.Vermifugo, p.DoseVermifugo, Aplicacoes(e.Dias, 150), p.FrascoVermifugo, p.PrecoVermifugo, "ml");
  Produto("Modificador orgânico", e.Modificador, p.DoseModificador, e.Dias, p.SacoModificador, p.PrecoModificador, "kg");
  var total = custos.Sum(c => c.Valor);
  var receita = arrobas * p.PrecoArroba;
  var lucro = receita - total;
  var margem = total == 0 ? (decimal?)null : lucro / total;
  var componentes = new List<Item>
  {
   new("Lotação", PontosLotacao(o), "pontos / 30", "30 entre 50–100%; redução linear fora da faixa."),
   new("Sanidade", (e.Carbunculo ? 8 : 0) + (e.Carrapato ? 7 : 0) + (e.Vermifugo ? 7 : 0) + (e.Modificador ? 3 : 0), "pontos / 25", "Carbúnculo 8; carrapato 7; vermífugo 7; modificador 3, quando selecionados."),
   new("Resultado financeiro", margem.HasValue ? PontosFinanceiros(margem.Value) : 0, "pontos / 30", "Interpolação entre margem −10% (0) e 20% (30)."),
   new("Manejo e custos fixos", (e.RacaPura ? 6 : 0) + (e.Dias is >= 120 and <= 365 ? 3 : 1) + (e.Posse == Posse.Proprio ? 2 : 0) + (efetiva ? 4 : 0), "pontos / 15", "Raça pura 6; prazo 120–365 dias 3, fora 1; próprio 2; rotação efetiva 4.")
  };
  void I(string n, decimal v, string u, string x = "") => indicadores.Add(new(n, v, u, x));
  I("UA do lote", ua, "UA"); I("Capacidade sem rotação", baseUa, "UA"); I("Capacidade utilizada", capacidade, "UA"); I("Ocupação", o * 100, "%");
  I("Animais compatíveis sem rotação", Math.Floor(baseUa * p.KgUa / e.PesoCompra), "animais", "Peso de compra como referência.");
  I("Animais compatíveis com manejo informado", Math.Floor(capacidade * p.KgUa / e.PesoCompra), "animais");
  I("GMD", gmd, "kg/dia"); I("Mortalidade", mortalidade * 100, "%"); I("Animais vendidos estimados", vendidos, "animais", "Estimativa estatística fracionária.");
  I("Peso de venda por animal", peso, "kg"); I("Peso total vendido", kg, "kg"); I("Arrobas vendidas", arrobas, "@ vivas"); I("Preço equivalente", p.PrecoArroba / p.KgArroba, "R$/kg");
  I("Gasto total", total, "R$"); I("Capital inicial", capital.Sum(c => c.Valor), "R$", "Exclui manutenção, arrendamento e suplementação."); I("Receita", receita, "R$"); I("Lucro estimado", lucro, "R$");
  if (margem.HasValue) I("Margem", margem.Value * 100, "%");
  if (kg > 0) I("Preço de equilíbrio", total / kg, "R$/kg");
  I("Receita com preço −10%", receita * .9m, "R$"); I("Lucro com preço −10%", receita * .9m - total, "R$");
  if (total > 0) I("Margem com preço −10%", (receita * .9m - total) / total * 100, "%");
  var alertas = new List<Alerta>();
  if (lucro < 0) alertas.Add(new("PREJUIZO", "Receita inferior aos custos. Revise compra, tamanho do lote e custos antes de investir."));
  if (o > 1) alertas.Add(new("SUPERLOTACAO", "O lote excede a capacidade estimada. Reduza a quantidade ou amplie a área."));
  if (o < .5m) alertas.Add(new("SUBUTILIZACAO", "Menos de metade da capacidade utilizada. Avalie os custos fixos por animal."));
  if (!e.Carbunculo || !e.Carrapato || !e.Vermifugo) alertas.Add(new("SANIDADE", "Há itens sanitários não selecionados. Revise o manejo com assistência veterinária."));
  if (e.Rotacao && !efetiva) alertas.Add(new("ROTACAO", "Menos de três piquetes gera custo, mas não aumenta a capacidade neste modelo."));
  if (margem < .2m) alertas.Add(new("MARGEM", "Margem abaixo de 20%. Avalie o cenário de queda do preço."));
  if (margem is null) alertas.Add(new("MARGEM_INDISPONIVEL", "Margem indisponível: gasto total zero."));
  if (kg <= 0) alertas.Add(new("EQUILIBRIO_INDISPONIVEL", "Equilíbrio indisponível: peso vendido zero."));
  var score = ArredondarScore(componentes.Sum(c => c.Valor));
  return new(score, Classificar(score), componentes, indicadores, custos, capital, alertas, p with { });
 }
}

