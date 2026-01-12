using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerzekeringApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Klanten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Voornaam = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tussenvoegsel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Achternaam = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Geboortedatum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Woonplaats = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    BeginDatum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EindDatum = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Klanten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Opstalverzekeringen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolisNummer = table.Column<int>(type: "INTEGER", nullable: false),
                    KlantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TypeDekking = table.Column<string>(type: "TEXT", maxLength: 75, nullable: false),
                    GedekteGebeurtenissen = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Uitsluitingen = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Herbouwwaarde = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Inboedelwaarde = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Premie = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Betaaltermijn = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AanvullendeOpties = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    BeginDatum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EindDatum = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opstalverzekeringen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opstalverzekeringen_Klanten_KlantId",
                        column: x => x.KlantId,
                        principalTable: "Klanten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opstalverzekeringen_KlantId",
                table: "Opstalverzekeringen",
                column: "KlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Opstalverzekeringen_PolisNummer",
                table: "Opstalverzekeringen",
                column: "PolisNummer",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Opstalverzekeringen");

            migrationBuilder.DropTable(
                name: "Klanten");
        }
    }
}
