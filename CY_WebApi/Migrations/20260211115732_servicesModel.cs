using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CY_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class servicesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CyGuarantee");

            migrationBuilder.DropIndex(
                name: "IX_CyUser_AccountId",
                table: "CyUser");

            migrationBuilder.CreateTable(
                name: "CyService",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductBrand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcceDevices = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductProblem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductProblemB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServicePrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CyService", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CyUser_AccountId",
                table: "CyUser",
                column: "AccountId",
                unique: true,
                filter: "[AccountId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CyService");

            migrationBuilder.DropIndex(
                name: "IX_CyUser_AccountId",
                table: "CyUser");

            migrationBuilder.CreateTable(
                name: "CyGuarantee",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CyUserID = table.Column<int>(type: "int", nullable: true),
                    CompanyExplaination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuaranteeCompany = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuaranteeID = table.Column<int>(type: "int", nullable: false),
                    GuarantreePrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductProblem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecievedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CyGuarantee", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CyGuarantee_CyUser_CyUserID",
                        column: x => x.CyUserID,
                        principalTable: "CyUser",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CyUser_AccountId",
                table: "CyUser",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CyGuarantee_CyUserID",
                table: "CyGuarantee",
                column: "CyUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CyGuarantee_GuaranteeID",
                table: "CyGuarantee",
                column: "GuaranteeID",
                unique: true);
        }
    }
}
