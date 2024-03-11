using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.AuthenticationLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedChatIdFieldForAppUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InChatWithId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InChatWithId",
                table: "AspNetUsers");
        }
    }
}
