using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrushHUB.Migrations
{
    /// <inheritdoc />
    public partial class AddUserConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserConfigJson",
                table: "UserReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserConfigJson",
                table: "Crashes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserConfigJson",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "UserConfigJson",
                table: "Crashes");
        }
    }
}
