using FluentResults;

namespace ScoreRebanho.Application;

public sealed record SalvarRebanhoDto(Guid? Id, Guid Versao, string Nome, string Propriedade, EntradaScoreDto Entrada);
public sealed record RebanhoDto(Guid Id, Guid Versao, string Nome, string Propriedade,
    DateTime CriadoEmUtc, DateTime AtualizadoEmUtc, EntradaScoreDto Entrada, ResultadoScoreDto Resultado);
public sealed record PaginaRebanhosDto(int Pagina, bool TemProxima, IReadOnlyList<RebanhoDto> Itens);
public interface IRebanhos
{
    Task<IReadOnlyList<RebanhoDto>> ListarAsync(string usuarioId, int pagina);
    Task<RebanhoDto?> ObterAsync(string usuarioId, Guid id);
    Task<bool> SalvarAsync(string usuarioId, RebanhoDto rebanho, Guid? versaoAnterior);
    Task<bool> ExcluirAsync(string usuarioId, Guid id, Guid versao);
}

public sealed class GerenciarRebanhos(IRebanhos repositorio, CalcularScoreRebanho calculadora)
{
    public async Task<Result<PaginaRebanhosDto>> ListarAsync(string usuarioId, int pagina)
    {
        if (string.IsNullOrWhiteSpace(usuarioId)) return Result.Fail<PaginaRebanhosDto>("Usuário não identificado.");
        pagina = Math.Clamp(pagina, 1, 1000000);
        var itens = await repositorio.ListarAsync(usuarioId, pagina);
        return Result.Ok(new PaginaRebanhosDto(pagina, itens.Count > 20, itens.Take(20).ToArray()));
    }
    public async Task<Result<RebanhoDto>> ObterAsync(string usuarioId, Guid id)
    {
        if (string.IsNullOrWhiteSpace(usuarioId)) return NaoEncontrado();
        var item = await repositorio.ObterAsync(usuarioId, id);
        return item is null ? NaoEncontrado() : Result.Ok(item);
    }
    public async Task<Result<RebanhoDto>> SalvarAsync(string usuarioId, SalvarRebanhoDto dados)
    {
        if (string.IsNullOrWhiteSpace(usuarioId)) return NaoEncontrado();
        RebanhoDto? atual = null;
        if (dados.Id.HasValue)
        {
            atual = await repositorio.ObterAsync(usuarioId, dados.Id.Value);
            if (atual is null) return NaoEncontrado();
            if (atual.Versao != dados.Versao) return Conflito();
        }
        var erros = new List<Error>();
        if (string.IsNullOrWhiteSpace(dados.Nome) || dados.Nome.Trim().Length > 100)
            erros.Add(new Error("Informe um nome de até 100 caracteres.").WithMetadata("Campo", "Nome"));
        if (string.IsNullOrWhiteSpace(dados.Propriedade) || dados.Propriedade.Trim().Length > 150)
            erros.Add(new Error("Informe uma propriedade de até 150 caracteres.").WithMetadata("Campo", "Propriedade"));
        if (erros.Count > 0) return Result.Fail<RebanhoDto>(erros);
        var resultado = calculadora.Executar(dados.Entrada);
        if (resultado.IsFailed)
            return Result.Fail<RebanhoDto>(resultado.Errors.Select(e =>
            {
                var erro = new Error(e.Message);
                if (e.Metadata.TryGetValue("Campo", out var campo)) erro.WithMetadata("Campo", $"Entrada.{campo}");
                return erro;
            }));
        var agora = DateTime.UtcNow;
        var rebanho = new RebanhoDto(atual?.Id ?? Guid.NewGuid(), Guid.NewGuid(), dados.Nome.Trim(), dados.Propriedade.Trim(),
            atual?.CriadoEmUtc ?? agora, agora, dados.Entrada, resultado.Value);
        return await repositorio.SalvarAsync(usuarioId, rebanho, atual?.Versao) ? Result.Ok(rebanho) : Conflito();
    }
    public async Task<Result> ExcluirAsync(string usuarioId, Guid id, Guid versao)
    {
        var atual = await ObterAsync(usuarioId, id);
        if (atual.IsFailed) return Result.Fail(atual.Errors);
        if (!await repositorio.ExcluirAsync(usuarioId, id, versao)) return Result.Fail(Conflito().Errors);
        return Result.Ok();
    }
    private static Result<RebanhoDto> NaoEncontrado() => Result.Fail<RebanhoDto>(new Error("Rebanho não encontrado.").WithMetadata("Codigo", "NAO_ENCONTRADO"));
    private static Result<RebanhoDto> Conflito() => Result.Fail<RebanhoDto>(new Error("Este rebanho foi alterado em outra sessão. Reabra o cadastro antes de continuar.").WithMetadata("Codigo", "CONFLITO"));
}
