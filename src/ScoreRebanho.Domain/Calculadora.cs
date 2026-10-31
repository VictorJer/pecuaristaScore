namespace ScoreRebanho.Domain;

public static class Calculadora
{
    public static decimal PontosLotacao(decimal ocupacao) => ocupacao < .5m ? 18 + 24 * ocupacao : ocupacao <= 1 ? 30 : Math.Max(0, 30 * (1.8m - ocupacao) / .8m);
    public static decimal PontosFinanceiros(decimal margem) => Math.Clamp((margem + .1m) * 100, 0, 30);
    public static int Aplicacoes(int dias, int intervalo) => checked(1 + dias / intervalo);
    public static int ArredondarScore(decimal score) => (int)Math.Round(Math.Clamp(score, 0, 100), 0, MidpointRounding.AwayFromZero);
    public static string Classificar(int score) => score >= 80 ? "Risco baixo" : score >= 60 ? "Risco moderado" : score >= 40 ? "Risco alto" : "Risco muito alto";
    public static Resultado Calcular(Entrada entrada, Premissas premissas)
    {
        if (entrada.Validar().Any()) throw new ArgumentException("Entradas inválidas.");
        premissas.Validar();
        var verao = entrada.Pastagem == Pastagem.Verao;
        var rotacaoEfetiva = entrada.Rotacao && entrada.Piquetes >= 3;
        var capacidadeSemRotacao = entrada.Area * (verao ? premissas.CapacidadeVerao : premissas.CapacidadeInverno);
        var capacidade = capacidadeSemRotacao * (rotacaoEfetiva ? Math.Min(1.6m, 1 + .15m * (entrada.Piquetes!.Value - 1)) : 1);
        var unidadesAnimais = entrada.Animais * entrada.PesoCompra / premissas.KgUa;
        var ocupacao = unidadesAnimais / capacidade;
        var excesso = Math.Max(0, ocupacao - 1);
        var gmd = (premissas.GmdBase + (entrada.RacaPura ? premissas.GmdRaca : 0) + (entrada.Carrapato ? premissas.GmdCarrapato : 0) + (entrada.Vermifugo ? premissas.GmdVermifugo : 0) + (entrada.Modificador ? premissas.GmdModificador : 0)) * Math.Max(.45m, 1 - .5m * excesso);
        var mortalidade = Math.Min(.15m, .015m + (entrada.Carbunculo ? 0 : .015m) + (entrada.Carrapato ? 0 : .012m) + (entrada.Vermifugo ? 0 : .008m) + .04m * excesso);
        var pesoVenda = entrada.PesoCompra + gmd * entrada.Dias;
        var animaisVendidos = entrada.Animais * (1 - mortalidade);
        var pesoTotalVendido = animaisVendidos * pesoVenda;
        var arrobas = pesoTotalVendido / premissas.KgArroba;
        var (custos, capital) = CalcularCustos(entrada, premissas, ocupacao);
        var indicadores = new List<Item>();
        var total = custos.Sum(c => c.Valor);
        var receita = arrobas * premissas.PrecoArroba;
        var lucro = receita - total;
        var margem = total == 0 ? (decimal?)null : lucro / total;
        var componentes = new List<Item>
  {
   new("Lotação", PontosLotacao(ocupacao), "pontos / 30", "30 entre 50–100%; redução linear fora da faixa."),
   new("Sanidade", (entrada.Carbunculo ? 8 : 0) + (entrada.Carrapato ? 7 : 0) + (entrada.Vermifugo ? 7 : 0) + (entrada.Modificador ? 3 : 0), "pontos / 25", "Carbúnculo 8; carrapato 7; vermífugo 7; modificador 3, quando selecionados."),
   new("Resultado financeiro", margem.HasValue ? PontosFinanceiros(margem.Value) : 0, "pontos / 30", "Interpolação entre margem −10% (0) e 20% (30)."),
   new("Manejo e custos fixos", (entrada.RacaPura ? 6 : 0) + (entrada.Dias is >= 120 and <= 365 ? 3 : 1) + (entrada.Posse == Posse.Proprio ? 2 : 0) + (rotacaoEfetiva ? 4 : 0), "pontos / 15", "Raça pura 6; prazo 120–365 dias 3, fora 1; próprio 2; rotação efetiva 4.")
  };
        void AdicionarIndicador(string nome, decimal valor, string unidade, string explicacao = "") => indicadores.Add(new(nome, valor, unidade, explicacao));
        AdicionarIndicador("UA do lote", unidadesAnimais, "UA"); AdicionarIndicador("Capacidade sem rotação", capacidadeSemRotacao, "UA"); AdicionarIndicador("Capacidade utilizada", capacidade, "UA"); AdicionarIndicador("Ocupação", ocupacao * 100, "%");
        AdicionarIndicador("Animais compatíveis sem rotação", Math.Floor(capacidadeSemRotacao * premissas.KgUa / entrada.PesoCompra), "animais", "Peso de compra como referência.");
        AdicionarIndicador("Animais compatíveis com manejo informado", Math.Floor(capacidade * premissas.KgUa / entrada.PesoCompra), "animais");
        AdicionarIndicador("GMD", gmd, "kg/dia"); AdicionarIndicador("Mortalidade", mortalidade * 100, "%"); AdicionarIndicador("Animais vendidos estimados", animaisVendidos, "animais", "Estimativa estatística fracionária.");
        AdicionarIndicador("Peso de venda por animal", pesoVenda, "kg"); AdicionarIndicador("Peso total vendido", pesoTotalVendido, "kg"); AdicionarIndicador("Arrobas vendidas", arrobas, "@ vivas"); AdicionarIndicador("Preço equivalente", premissas.PrecoArroba / premissas.KgArroba, "R$/kg");
        AdicionarIndicador("Gasto total", total, "R$"); AdicionarIndicador("Capital inicial", capital.Sum(c => c.Valor), "R$", "Exclui manutenção, arrendamento e suplementação."); AdicionarIndicador("Receita", receita, "R$"); AdicionarIndicador("Lucro estimado", lucro, "R$");
        if (margem.HasValue) AdicionarIndicador("Margem", margem.Value * 100, "%");
        if (pesoTotalVendido > 0) AdicionarIndicador("Preço de equilíbrio", total / pesoTotalVendido, "R$/kg");
        AdicionarIndicador("Receita com preço −10%", receita * .9m, "R$"); AdicionarIndicador("Lucro com preço −10%", receita * .9m - total, "R$");
        if (total > 0) AdicionarIndicador("Margem com preço −10%", (receita * .9m - total) / total * 100, "%");
        var alertas = GerarAlertas(entrada, rotacaoEfetiva, ocupacao, lucro, margem, pesoTotalVendido);
        var score = ArredondarScore(componentes.Sum(c => c.Valor));
        return new(score, Classificar(score), componentes, indicadores, custos, capital, alertas, premissas with { });
    }

    private static (List<Item> Custos, List<Item> Capital) CalcularCustos(Entrada entrada, Premissas premissas, decimal ocupacao)
    {
        var verao = entrada.Pastagem == Pastagem.Verao;
        var custos = new List<Item>();
        var capital = new List<Item>();

        void Custo(string nome, decimal valor, string explicacao) => custos.Add(new(nome, valor, "R$", explicacao));
        Custo("Compra do gado", entrada.Animais * entrada.PesoCompra * entrada.PrecoCompra, "Lote inicial × peso × preço/kg.");
        capital.Add(custos[^1]);
        Custo("Manutenção do pasto", entrada.Area * (verao ? premissas.ManutencaoVerao : premissas.ManutencaoInverno) * entrada.Dias / 365, "Custo anual × hectares × dias/365.");
        var formacao = entrada.PastoPronto ? 0 : entrada.Area * (verao ? premissas.FormacaoVerao : premissas.FormacaoInverno);
        Custo("Formação do pasto", formacao / (verao ? 5 : 1), "Verão: 1/5 do investimento; inverno: integral.");
        capital.Add(new("Formação integral", formacao, "R$", "Investimento quando o pasto não está pronto."));
        var rotacao = entrada.Rotacao ? entrada.Area * (premissas.RotacaoBase + premissas.RotacaoPiquete * entrada.Piquetes!.Value) : 0;
        Custo("Rotação amortizada", rotacao * entrada.Dias / 3650, "Investimento × dias/3650, mesmo com menos de três piquetes.");
        capital.Add(new("Rotação integral", rotacao, "R$", "Área × (custo base + custo por piquete × piquetes)."));
        Custo("Arrendamento", entrada.Posse == Posse.Arrendado ? entrada.ArrendamentoMensal!.Value * entrada.Dias / 30 : 0, "Mensalidade × dias/30.");
        Custo("Suplementação", entrada.Animais * (decimal)entrada.Dias * premissas.SuplementoDia * Math.Max(0, ocupacao - 1.10m), "Animais × dias × tarifa × máximo(0, ocupação decimal − 1,10).");
        void Produto(string nome, bool ativo, decimal dose, int aplicacoes, decimal embalagem, decimal preco, string unidade)
        {
            var volume = ativo ? entrada.Animais * dose * aplicacoes : 0;
            var frascos = Math.Ceiling(volume / embalagem);
            Custo(nome, frascos * preco, $"{(ativo ? aplicacoes : 0)} aplicações/dias; {volume} {unidade}; {frascos} embalagens de {embalagem} {unidade}.");
            capital.Add(custos[^1]);
        }
        Produto("Carbúnculo", entrada.Carbunculo, premissas.DoseCarbunculo, 1, premissas.FrascoCarbunculo, premissas.PrecoCarbunculo, "ml");
        Produto("Carrapato", entrada.Carrapato, premissas.DoseCarrapato, Aplicacoes(entrada.Dias, 90), premissas.FrascoCarrapato, premissas.PrecoCarrapato, "ml");
        Produto("Vermífugo", entrada.Vermifugo, premissas.DoseVermifugo, Aplicacoes(entrada.Dias, 150), premissas.FrascoVermifugo, premissas.PrecoVermifugo, "ml");
        Produto("Modificador orgânico", entrada.Modificador, premissas.DoseModificador, entrada.Dias, premissas.SacoModificador, premissas.PrecoModificador, "kg");
        return (custos, capital);
    }
    private static List<Alerta> GerarAlertas(Entrada entrada, bool rotacaoEfetiva, decimal ocupacao, decimal lucro, decimal? margem, decimal pesoTotalVendido)
    {
        var alertas = new List<Alerta>();
        if (lucro < 0) alertas.Add(new("PREJUIZO", "Receita inferior aos custos. Revise compra, tamanho do lote e custos antes de investir."));
        if (ocupacao > 1) alertas.Add(new("SUPERLOTACAO", "O lote excede a capacidade estimada. Reduza a quantidade ou amplie a área."));
        if (ocupacao < .5m) alertas.Add(new("SUBUTILIZACAO", "Menos de metade da capacidade utilizada. Avalie os custos fixos por animal."));
        if (!entrada.Carbunculo || !entrada.Carrapato || !entrada.Vermifugo) alertas.Add(new("SANIDADE", "Há itens sanitários não selecionados. Revise o manejo com assistência veterinária."));
        if (entrada.Rotacao && !rotacaoEfetiva) alertas.Add(new("ROTACAO", "Menos de três piquetes gera custo, mas não aumenta a capacidade neste modelo."));
        if (margem < .2m) alertas.Add(new("MARGEM", "Margem abaixo de 20%. Avalie o cenário de queda do preço."));
        if (margem is null) alertas.Add(new("MARGEM_INDISPONIVEL", "Margem indisponível: gasto total zero."));
        if (pesoTotalVendido <= 0) alertas.Add(new("EQUILIBRIO_INDISPONIVEL", "Equilíbrio indisponível: peso vendido zero."));
        // Ordem de atenção da interface v6; não altera pesos ou classificação do score.
        string[] ordem = ["PREJUIZO", "SUPERLOTACAO", "SANIDADE", "MARGEM_INDISPONIVEL", "EQUILIBRIO_INDISPONIVEL", "MARGEM", "ROTACAO", "SUBUTILIZACAO"];
        return alertas.OrderBy(a => Array.IndexOf(ordem, a.Codigo)).ToList();
    }
}
