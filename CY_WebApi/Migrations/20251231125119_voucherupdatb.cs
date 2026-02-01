using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CY_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class voucherupdatb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherItem_Account_AccountId",
                table: "VoucherItem");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherItem_Voucher_VoucherId",
                table: "VoucherItem");

            migrationBuilder.AlterColumn<int>(
                name: "VoucherId",
                table: "VoucherItem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "VoucherItem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AccountID",
                table: "Voucher",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Voucher_AccountID",
                table: "Voucher",
                column: "AccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_Voucher_Account_AccountID",
                table: "Voucher",
                column: "AccountID",
                principalTable: "Account",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherItem_Account_AccountId",
                table: "VoucherItem",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherItem_Voucher_VoucherId",
                table: "VoucherItem",
                column: "VoucherId",
                principalTable: "Voucher",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voucher_Account_AccountID",
                table: "Voucher");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherItem_Account_AccountId",
                table: "VoucherItem");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherItem_Voucher_VoucherId",
                table: "VoucherItem");

            migrationBuilder.DropIndex(
                name: "IX_Voucher_AccountID",
                table: "Voucher");

            migrationBuilder.DropColumn(
                name: "AccountID",
                table: "Voucher");

            migrationBuilder.AlterColumn<int>(
                name: "VoucherId",
                table: "VoucherItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "VoucherItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherItem_Account_AccountId",
                table: "VoucherItem",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherItem_Voucher_VoucherId",
                table: "VoucherItem",
                column: "VoucherId",
                principalTable: "Voucher",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
