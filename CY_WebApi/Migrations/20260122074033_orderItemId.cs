using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CY_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class orderItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "CyOrderItem");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<int>(
                name: "CyOrderID",
                table: "CyOrderItem",
                type: "int",
                nullable: true);

     
        }
    }
}
