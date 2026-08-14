using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrushHUB.Migrations
{
    /// <inheritdoc />
    public partial class AddProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "161B548E-0A90-43E0-A76E-0F34C60955B0",
                column: "ConcurrencyStamp",
                value: "521d70ab-4374-4e03-b939-535e4086198e");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4B00D67B-169D-459D-8BE0-5A1F9575F247",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "f46a6c1e-c201-4b8c-bca7-e39627f4e8ad", "AQAAAAIAAYagAAAAEH9/P9sjnqQYmtdLt4WYqMyiUKm+/CJ1l0+xpKP+nZQkWEEA13l064nLv25vllnFXQ==" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ApiKey",
                table: "Projects",
                column: "ApiKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "161B548E-0A90-43E0-A76E-0F34C60955B0",
                column: "ConcurrencyStamp",
                value: "ea9a335e-cc2a-4f51-a3a4-a2b70cfed00c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4B00D67B-169D-459D-8BE0-5A1F9575F247",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "24f30696-1879-4940-a627-27ae9205b5c3", "AQAAAAIAAYagAAAAED6uS04D0XLP+nS9iQV3p2ztMx78aXzUZNKCytjtK5z7UZQhPp3l0slBkWZxm8gJIA==" });
        }
    }
}
