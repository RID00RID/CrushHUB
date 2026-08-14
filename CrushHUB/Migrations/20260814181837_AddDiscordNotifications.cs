using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrushHUB.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordWebhookUrl",
                table: "Projects",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnCrash",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnReport",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordWebhookUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "NotifyOnCrash",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "NotifyOnReport",
                table: "Projects");
        }
    }
}
