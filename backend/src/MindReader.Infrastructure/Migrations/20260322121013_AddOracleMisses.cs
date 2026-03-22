using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindReader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOracleMisses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OracleMisses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OracleGuess = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OracleMisses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OracleMisses");
        }
    }
}
