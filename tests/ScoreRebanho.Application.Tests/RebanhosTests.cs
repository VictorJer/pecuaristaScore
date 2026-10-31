using ScoreRebanho.Domain;

namespace ScoreRebanho.Application.Tests;

[TestClass]
public class RebanhosTests
{
    private sealed class Provedor : IProvedorPremissas { public Premissas Obter() => new(); }
    private sealed class Memoria : IRebanhos
    {
        public RebanhoDto? Atual { get; set; }
        public bool AceitarGravacao { get; set; } = true;
        public int Gravacoes { get; private set; }
        public Task<IReadOnlyList<RebanhoDto>> ListarAsync(string usuarioId, int pagina) => Task.FromResult<IReadOnlyList<RebanhoDto>>([]);
        public Task<RebanhoDto?> ObterAsync(string usuarioId, Guid id) => Task.FromResult(Atual?.Id == id && usuarioId == "autor" ? Atual : null);
        public Task<bool> SalvarAsync(string usuarioId, RebanhoDto rebanho, Guid? versaoAnterior)
        {
            Gravacoes++;
            if (AceitarGravacao) Atual = rebanho;
            return Task.FromResult(AceitarGravacao);
        }
        public Task<bool> ExcluirAsync(string usuarioId, Guid id, Guid versao) => Task.FromResult(false);
    }
    private static SalvarRebanhoDto Entrada => new(null, Guid.Empty, "  Lote  ", "  Fazenda  ",
        new() { Area = 10, Animais = 10, PesoCompra = 450, PrecoCompra = 5, Dias = 180 });

    [TestMethod]
    public async Task CriaNormalizaNomeERecalculaEdicao()
    {
        var banco = new Memoria();
        var servico = new GerenciarRebanhos(banco, new(new Provedor()));
        var criado = await servico.SalvarAsync("autor", Entrada);
        Assert.IsTrue(criado.IsSuccess);
        Assert.AreEqual("Lote", criado.Value.Nome);
        Assert.AreEqual("Fazenda", criado.Value.Propriedade);
        var editado = await servico.SalvarAsync("autor", Entrada with { Id = criado.Value.Id, Versao = criado.Value.Versao, Entrada = Entrada.Entrada with { Animais = 20 } });
        Assert.IsTrue(editado.IsSuccess);
        Assert.AreEqual(criado.Value.Id, editado.Value.Id);
        Assert.AreNotEqual(criado.Value.Versao, editado.Value.Versao);
        Assert.AreEqual(200m, editado.Value.Resultado.Indicadores.Single(i => i.Nome == "Ocupação").Valor);
    }
    [TestMethod]
    public async Task RejeitaValoresInvalidosSemGravar()
    {
        var banco = new Memoria();
        var servico = new GerenciarRebanhos(banco, new(new Provedor()));
        Assert.IsTrue((await servico.SalvarAsync("autor", Entrada with { Nome = new string('x', 101) })).IsFailed);
        Assert.IsTrue((await servico.SalvarAsync("autor", Entrada with { Propriedade = " " })).IsFailed);
        Assert.IsTrue((await servico.SalvarAsync("autor", Entrada with { Entrada = Entrada.Entrada with { Animais = 0 } })).IsFailed);
        Assert.IsTrue((await servico.SalvarAsync("autor", Entrada with { Entrada = Entrada.Entrada with { PesoCompra = decimal.MaxValue } })).IsFailed);
        Assert.AreEqual(0, banco.Gravacoes);
    }
    [TestMethod]
    public async Task ConflitoEntreLeituraEGravacaoViraFalhaEsperada()
    {
        var banco = new Memoria();
        var servico = new GerenciarRebanhos(banco, new(new Provedor()));
        var criado = (await servico.SalvarAsync("autor", Entrada)).Value;
        banco.AceitarGravacao = false;
        var resultado = await servico.SalvarAsync("autor", Entrada with { Id = criado.Id, Versao = criado.Versao });
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("CONFLITO", resultado.Errors[0].Metadata["Codigo"]);
        Assert.AreEqual(criado, banco.Atual);
        Assert.IsTrue((await servico.ObterAsync("outro", criado.Id)).IsFailed);
        Assert.IsTrue((await servico.ExcluirAsync("outro", criado.Id, criado.Versao)).IsFailed);
    }
}
