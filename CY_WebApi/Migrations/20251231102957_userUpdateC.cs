using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CY_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class userUpdateC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "CyUser",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CyUser_AccountId",
                table: "CyUser",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_CyUser_Account_AccountId",
                table: "CyUser",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CyUser_Account_AccountId",
                table: "CyUser");

            migrationBuilder.DropIndex(
                name: "IX_CyUser_AccountId",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "CyUser");
        }
    }
}
