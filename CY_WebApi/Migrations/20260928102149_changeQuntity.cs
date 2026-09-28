using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CY_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class changeQuntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChangeQuantType",
                table: "CyOrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "CyOrderItem",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityChange",
                table: "CyOrderItem",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeQuantType",
                table: "CyOrderItem");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "CyOrderItem");

            migrationBuilder.DropColumn(
                name: "QuantityChange",
                table: "CyOrderItem");
        }
    }
}
