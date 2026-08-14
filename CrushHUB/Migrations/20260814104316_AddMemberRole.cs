using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrushHUB.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "2F5F5C1E-3A65-4F0C-9E5F-9F2A0B6C4D11", "8c1de6f0-5f2b-4b8a-9a4c-1d0f6b7e2a33", "member", "MEMBER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2F5F5C1E-3A65-4F0C-9E5F-9F2A0B6C4D11");
        }
    }
}
