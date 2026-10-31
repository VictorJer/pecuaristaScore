using FluentResults;
using ScoreRebanho.Domain;

namespace ScoreRebanho.Application;

public interface IProvedorPremissas
{
    Premissas Obter();
}

public sealed class CalcularScoreRebanho(IProvedorPremissas provedor)
{
    public Result<ResumoScoreDto> Resumir(EntradaScoreDto dados)
    {
        var resultado = Executar(dados);
        if (resultado.IsFailed) return Result.Fail<ResumoScoreDto>(resultado.Errors);
        var indicadores = resultado.Value.Indicadores;
        return Result.Ok(new ResumoScoreDto(dados.Area, dados.Animais,
            indicadores.Single(i => i.Nome == "UA do lote").Valor,
            indicadores.Single(i => i.Nome == "Capacidade utilizada").Valor,
            indicadores.Single(i => i.Nome == "Ocupação").Valor));
    }

    public Result<ResultadoScoreDto> Executar(EntradaScoreDto dados)
    {
        var entrada = CriarEntrada(dados);
        var erros = entrada.Validar()
            .Select(erro => new Error(erro.Mensagem)
                .WithMetadata("Campo", erro.Campo)
                .WithMetadata("Codigo", "ENTRADA_INVALIDA"))
            .ToArray();

        if (erros.Length > 0) return Result.Fail<ResultadoScoreDto>(erros);

        try
        {
            var resultado = Calculadora.Calcular(entrada, provedor.Obter());
            return Result.Ok(CriarResposta(resultado));
        }
        catch (DivideByZeroException)
        {
            return FalhaNumerica("CALCULO_INDISPONIVEL",
                "Não foi possível calcular com estes valores: capacidade inferior à precisão numérica suportada.");
        }
        catch (OverflowException)
        {
            return FalhaNumerica("LIMITE_NUMERICO",
                "Valores excedem a capacidade de cálculo. Reduza os valores informados.");
        }
    }

    private static Result<ResultadoScoreDto> FalhaNumerica(string codigo, string mensagem) =>
        Result.Fail<ResultadoScoreDto>(new Error(mensagem).WithMetadata("Codigo", codigo));

    private static Entrada CriarEntrada(EntradaScoreDto dados) => new()
    {
        Area = dados.Area,
        Posse = (Posse)dados.Posse,
        ArrendamentoMensal = dados.ArrendamentoMensal,
        Pastagem = (Pastagem)dados.Pastagem,
        PastoPronto = dados.PastoPronto,
        Rotacao = dados.Rotacao,
        Piquetes = dados.Piquetes,
        Animais = dados.Animais,
        PesoCompra = dados.PesoCompra,
        PrecoCompra = dados.PrecoCompra,
        Dias = dados.Dias,
        RacaPura = dados.RacaPura,
        Carbunculo = dados.Carbunculo,
        Carrapato = dados.Carrapato,
        Vermifugo = dados.Vermifugo,
        Modificador = dados.Modificador
    };

    private static ResultadoScoreDto CriarResposta(Resultado resultado)
    {
        var cultura = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        var premissas = typeof(Premissas).GetProperties().ToDictionary(
            propriedade => propriedade.Name,
            propriedade => Convert.ToString(propriedade.GetValue(resultado.Premissas), cultura) ?? "");

        return new ResultadoScoreDto(
            resultado.Score,
            resultado.Classificacao,
            MapearItens(resultado.Componentes),
            MapearItens(resultado.Indicadores),
            MapearItens(resultado.Custos),
            MapearItens(resultado.Capital),
            resultado.Alertas.Select(alerta => new AlertaDto(alerta.Codigo, alerta.Mensagem)).ToArray(),
            resultado.Premissas.Versao,
            premissas);
    }

    private static ItemDto[] MapearItens(IEnumerable<Item> itens) =>
        itens.Select(item => new ItemDto(item.Nome, item.Valor, item.Unidade, item.Explicacao)).ToArray();
}
