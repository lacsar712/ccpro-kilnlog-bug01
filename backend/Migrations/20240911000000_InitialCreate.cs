using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KilnLog.Api.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Studios",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                City = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Notes = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Studios", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: false),
                Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Kilns",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudioId = table.Column<int>(type: "integer", nullable: false),
                KilnCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                MaxTempC = table.Column<int>(type: "integer", nullable: false),
                FuelType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Kilns", x => x.Id);
                table.ForeignKey(
                    name: "FK_Kilns_Studios_StudioId",
                    column: x => x.StudioId,
                    principalTable: "Studios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FiringSchedules",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                KilnId = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ConeOrTarget = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FiringSchedules", x => x.Id);
                table.ForeignKey(
                    name: "FK_FiringSchedules_Kilns_KilnId",
                    column: x => x.KilnId,
                    principalTable: "Kilns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ScheduleSegments",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ScheduleId = table.Column<int>(type: "integer", nullable: false),
                Seq = table.Column<int>(type: "integer", nullable: false),
                RampCPerHour = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                HoldMinutes = table.Column<int>(type: "integer", nullable: false),
                TargetTempC = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ScheduleSegments", x => x.Id);
                table.ForeignKey(
                    name: "FK_ScheduleSegments_FiringSchedules_ScheduleId",
                    column: x => x.ScheduleId,
                    principalTable: "FiringSchedules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LoadBatches",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                KilnId = table.Column<int>(type: "integer", nullable: false),
                ScheduleId = table.Column<int>(type: "integer", nullable: false),
                LoadDate = table.Column<DateOnly>(type: "date", nullable: false),
                PieceCount = table.Column<int>(type: "integer", nullable: false),
                GlazeNotes = table.Column<string>(type: "text", nullable: true),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LoadBatches", x => x.Id);
                table.ForeignKey(
                    name: "FK_LoadBatches_FiringSchedules_ScheduleId",
                    column: x => x.ScheduleId,
                    principalTable: "FiringSchedules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_LoadBatches_Kilns_KilnId",
                    column: x => x.KilnId,
                    principalTable: "Kilns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_FiringSchedules_KilnId",
            table: "FiringSchedules",
            column: "KilnId");

        migrationBuilder.CreateIndex(
            name: "IX_Kilns_StudioId_KilnCode",
            table: "Kilns",
            columns: new[] { "StudioId", "KilnCode" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LoadBatches_KilnId",
            table: "LoadBatches",
            column: "KilnId");

        migrationBuilder.CreateIndex(
            name: "IX_LoadBatches_ScheduleId",
            table: "LoadBatches",
            column: "ScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleSegments_ScheduleId_Seq",
            table: "ScheduleSegments",
            columns: new[] { "ScheduleId", "Seq" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Username",
            table: "Users",
            column: "Username",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LoadBatches");
        migrationBuilder.DropTable(name: "ScheduleSegments");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.DropTable(name: "FiringSchedules");
        migrationBuilder.DropTable(name: "Kilns");
        migrationBuilder.DropTable(name: "Studios");
    }
}
