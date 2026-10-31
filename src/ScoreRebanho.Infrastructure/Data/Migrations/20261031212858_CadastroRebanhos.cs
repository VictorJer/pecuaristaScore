using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScoreRebanho.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CadastroRebanhos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rebanhos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Versao = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Propriedade = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEmUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EntradaJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResultadoJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rebanhos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rebanhos_UsuarioId_AtualizadoEmUtc",
                table: "Rebanhos",
                columns: new[] { "UsuarioId", "AtualizadoEmUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rebanhos");
        }
    }
}
