using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScoreRebanho.Application;
using ScoreRebanho.Infrastructure.Data;
using ScoreRebanho.Web.Models;
namespace ScoreRebanho.Web.Tests;
[TestClass]
public class IntegracaoTests
{
 [TestMethod]
 public void MapeamentosSaoValidos()
 {
  var config=new MapperConfiguration(c=>c.AddProfile<SimulacaoProfile>(),NullLoggerFactory.Instance);
  config.AssertConfigurationIsValid();
  var dto=config.CreateMapper().Map<EntradaScoreDto>(new SimulacaoVm { Area=12.5m,Animais=8,Rotacao=true,Piquetes=3 });
  Assert.AreEqual(12.5m,dto.Area); Assert.AreEqual(8,dto.Animais); Assert.AreEqual(3,dto.Piquetes);
 }
 [TestMethod]
 public async Task MigrationCriaIdentityEmSqlite()
 {
  await using var connection=new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
  var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
  await using var db=new ApplicationDbContext(options);
  await db.Database.MigrateAsync();
  db.Users.Add(new IdentityUser { UserName="teste@example.test",Email="teste@example.test" });
  await db.SaveChangesAsync(); db.ChangeTracker.Clear();
  Assert.AreEqual(1,await db.Users.CountAsync());
  Assert.IsFalse((await db.Database.GetPendingMigrationsAsync()).Any());
 }
}
