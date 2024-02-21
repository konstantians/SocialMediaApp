using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserIdToPostVotesEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PostVotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PostVotes");
        }
    }
}
