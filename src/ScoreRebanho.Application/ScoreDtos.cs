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

