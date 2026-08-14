using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrushHUB.Migrations
{
    /// <inheritdoc />
    public partial class AddGameUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserConfigJson",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "UserConfigJson",
                table: "Crashes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Crashes");

            migrationBuilder.AddColumn<int>(
                name: "GameUserId",
                table: "UserReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameUserId",
                table: "Crashes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    SystemId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OsName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OsVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cpu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Gpu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MemoryMb = table.Column<int>(type: "int", nullable: true),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameUsers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_GameUserId",
                table: "UserReports",
                column: "GameUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Crashes_GameUserId",
                table: "Crashes",
                column: "GameUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameUsers_ProjectId_SystemId",
                table: "GameUsers",
                columns: new[] { "ProjectId", "SystemId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Crashes_GameUsers_GameUserId",
                table: "Crashes",
                column: "GameUserId",
                principalTable: "GameUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_GameUsers_GameUserId",
                table: "UserReports",
                column: "GameUserId",
                principalTable: "GameUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Crashes_GameUsers_GameUserId",
                table: "Crashes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_GameUsers_GameUserId",
                table: "UserReports");

            migrationBuilder.DropTable(
                name: "GameUsers");

            migrationBuilder.DropIndex(
                name: "IX_UserReports_GameUserId",
                table: "UserReports");

            migrationBuilder.DropIndex(
                name: "IX_Crashes_GameUserId",
                table: "Crashes");

            migrationBuilder.DropColumn(
                name: "GameUserId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "GameUserId",
                table: "Crashes");

            migrationBuilder.AddColumn<string>(
                name: "UserConfigJson",
                table: "UserReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserReports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserConfigJson",
                table: "Crashes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Crashes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
