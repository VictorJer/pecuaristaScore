namespace ScoreRebanho.Web.Models;

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
    public IReadOnlyDictionary<string, string> Premissas { get; set; } = new Dictionary<string, string>();
    public SimulacaoVm Entrada { get; set; } = new();
}

