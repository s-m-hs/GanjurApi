using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CY_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class vorchereditr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToAccountId",
                table: "VoucherItem",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToAccountId",
                table: "VoucherItem");
        }
    }
}
