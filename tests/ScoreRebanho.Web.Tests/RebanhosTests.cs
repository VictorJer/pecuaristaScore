using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScoreRebanho.Application;
using ScoreRebanho.Domain;
using ScoreRebanho.Infrastructure;
using ScoreRebanho.Infrastructure.Data;
using ScoreRebanho.Web.Models;

namespace ScoreRebanho.Web.Tests;

[TestClass]
public class RebanhosTests
{
    private sealed class Provedor : IProvedorPremissas { public Premissas Obter() => new(); }
    private static SalvarRebanhoDto Entrada => new(null, Guid.Empty, "Lote verão", "Fazenda Boa Vista",
        new() { Area = 10, Animais = 10, PesoCompra = 450, PrecoCompra = 5, Dias = 180 });

    [TestMethod]
    public void MapeamentosPreservamDadosEIdentidade()
    {
        var config = new MapperConfiguration(c => { c.AddProfile<SimulacaoProfile>(); c.AddProfile<RebanhosProfile>(); }, NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();
        var id = Guid.NewGuid();
        var versao = Guid.NewGuid();
        var dto = mapper.Map<SalvarRebanhoDto>(new RebanhoFormularioVm { Id = id, Versao = versao, Nome = "Lote", Propriedade = "Fazenda", Entrada = new() { Area = 12.5m, Animais = 9 } });
        Assert.AreEqual(id, dto.Id);
        Assert.AreEqual(versao, dto.Versao);
        Assert.AreEqual(12.5m, dto.Entrada.Area);
        Assert.AreEqual(9, dto.Entrada.Animais);
    }

    [TestMethod]
    public async Task CrudIsolaContasRecalculaEPreservaHistorico()
    {
        await using var conexao = new SqliteConnection("Data Source=:memory:");
        await conexao.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexao).Options);
        await db.Database.MigrateAsync();
        var repositorio = new Rebanhos(db);
        var servico = new GerenciarRebanhos(repositorio, new(new Provedor()));
        var criado = await servico.SalvarAsync("autor", Entrada);
        Assert.IsTrue(criado.IsSuccess);
        var rebanho = criado.Value;
        db.ChangeTracker.Clear();
        Assert.HasCount(1, (await servico.ListarAsync("autor", 1)).Value.Itens);
        Assert.IsEmpty((await servico.ListarAsync("outro", 1)).Value.Itens);
        Assert.IsTrue((await servico.ObterAsync("outro", rebanho.Id)).IsFailed);
        var editar = Entrada with { Id = rebanho.Id, Versao = rebanho.Versao, Nome = "Atualizado", Entrada = Entrada.Entrada with { Animais = 20 } };
        Assert.IsTrue((await servico.SalvarAsync("outro", editar)).IsFailed);
        Assert.IsFalse(await repositorio.ExcluirAsync("outro", rebanho.Id, rebanho.Versao));
        Assert.IsFalse(await repositorio.SalvarAsync("outro", rebanho, rebanho.Versao));
        Assert.IsTrue((await servico.ExcluirAsync("outro", rebanho.Id, rebanho.Versao)).IsFailed);
        var atualizado = await servico.SalvarAsync("autor", editar);
        Assert.IsTrue(atualizado.IsSuccess);
        Assert.AreEqual(20, atualizado.Value.Entrada.Animais);
        Assert.AreEqual(200m, atualizado.Value.Resultado.Indicadores.Single(i => i.Nome == "Ocupação").Valor);
        Assert.AreEqual(rebanho.CriadoEmUtc, atualizado.Value.CriadoEmUtc);
        Assert.IsTrue((await servico.SalvarAsync("autor", editar)).IsFailed);
        Assert.IsTrue((await servico.ExcluirAsync("autor", rebanho.Id, rebanho.Versao)).IsFailed);
        Assert.IsTrue((await servico.ExcluirAsync("autor", rebanho.Id, atualizado.Value.Versao)).IsSuccess);
        Assert.IsTrue((await servico.ObterAsync("autor", rebanho.Id)).IsFailed);
        Assert.AreEqual(2, await db.Set<ScorePersistido>().CountAsync());
        Assert.IsFalse(db.Database.HasPendingModelChanges());
    }

    [TestMethod]
    public async Task ValidacaoNaoPersisteEListaPaginadaTemProxima()
    {
        await using var conexao = new SqliteConnection("Data Source=:memory:");
        await conexao.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexao).Options);
        await db.Database.MigrateAsync();
        var servico = new GerenciarRebanhos(new Rebanhos(db), new(new Provedor()));
        Assert.IsTrue((await servico.SalvarAsync("", Entrada)).IsFailed);
        Assert.IsTrue((await servico.SalvarAsync("autor", Entrada with { Nome = " " })).IsFailed);
        Assert.IsTrue((await servico.SalvarAsync("autor", Entrada with { Propriedade = new string('a', 151) })).IsFailed);
        var invalido = await servico.SalvarAsync("autor", Entrada with { Entrada = Entrada.Entrada with { Area = 0 } });
        Assert.IsTrue(invalido.IsFailed);
        Assert.AreEqual("Entrada.Area", invalido.Errors[0].Metadata["Campo"]);
        Assert.AreEqual(0, await db.Set<RebanhoPersistido>().CountAsync());
        Assert.AreEqual(0, await db.Set<ScorePersistido>().CountAsync());
        for (var i = 0; i < 21; i++) Assert.IsTrue((await servico.SalvarAsync("autor", Entrada with { Nome = $"Lote {i}" })).IsSuccess);
        var primeira = (await servico.ListarAsync("autor", 0)).Value;
        var segunda = (await servico.ListarAsync("autor", 2)).Value;
        Assert.HasCount(20, primeira.Itens);
        Assert.IsTrue(primeira.TemProxima);
        Assert.HasCount(1, segunda.Itens);
        Assert.IsFalse(segunda.TemProxima);
        Assert.IsFalse(primeira.Itens.Any(i => i.Id == segunda.Itens[0].Id));
    }
}
