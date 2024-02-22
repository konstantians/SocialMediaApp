using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.AuthenticationLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RemovedForeignKeysFromFriendship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_FriendId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_FriendId",
                table: "Friendships");

            migrationBuilder.AlterColumn<string>(
                name: "FriendId",
                table: "Friendships",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Friendships",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_AppUserId",
                table: "Friendships",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_AppUserId",
                table: "Friendships",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_AppUserId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_AppUserId",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Friendships");

            migrationBuilder.AlterColumn<string>(
                name: "FriendId",
                table: "Friendships",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_FriendId",
                table: "Friendships",
                column: "FriendId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_FriendId",
                table: "Friendships",
                column: "FriendId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
