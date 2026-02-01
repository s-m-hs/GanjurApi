using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CY_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class userModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CyUser_CyProfile_CyProfileID",
                table: "CyUser");

            migrationBuilder.DropIndex(
                name: "IX_CyUser_CyProfileID",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "CyProfileID",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "LastLoginTime",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "MaxWrongPass",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "RegisterCode",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "RegisterCodeTimer",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "WrongPass",
                table: "CyUser");

            migrationBuilder.RenameColumn(
                name: "TTKK",
                table: "CyUser",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "parentId",
                table: "CyProductCategory",
                newName: "ParentId");

            migrationBuilder.AddColumn<string>(
                name: "MelliCode",
                table: "CyUser",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "CyUser",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MelliCode",
                table: "CyUser");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "CyUser");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "CyUser",
                newName: "TTKK");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "CyProductCategory",
                newName: "parentId");

            migrationBuilder.AddColumn<int>(
                name: "CyProfileID",
                table: "CyUser",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginTime",
                table: "CyUser",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxWrongPass",
                table: "CyUser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RegisterCode",
                table: "CyUser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisterCodeTimer",
                table: "CyUser",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WrongPass",
                table: "CyUser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CyUser_CyProfileID",
                table: "CyUser",
                column: "CyProfileID");

            migrationBuilder.AddForeignKey(
                name: "FK_CyUser_CyProfile_CyProfileID",
                table: "CyUser",
                column: "CyProfileID",
                principalTable: "CyProfile",
                principalColumn: "ID");
        }
    }
}
