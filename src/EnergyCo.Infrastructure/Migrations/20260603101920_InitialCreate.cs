using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscountPromotionProducts",
                columns: table => new
                {
                    DiscountPromotionId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProductId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountPromotionProducts", x => new { x.DiscountPromotionId, x.ProductId });
                });

            migrationBuilder.CreateTable(
                name: "DiscountPromotions",
                columns: table => new
                {
                    DiscountPromotionId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountPromotions", x => x.DiscountPromotionId);
                });

            migrationBuilder.CreateTable(
                name: "PointsPromotions",
                columns: table => new
                {
                    PointsPromotionId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PointsPerDollar = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculationBasis = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsPromotions", x => x.PointsPromotionId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountPromotionProducts_ProductId",
                table: "DiscountPromotionProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountPromotions_StartDateUtc_EndDateUtc",
                table: "DiscountPromotions",
                columns: new[] { "StartDateUtc", "EndDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PointsPromotions_StartDateUtc_EndDateUtc_Category",
                table: "PointsPromotions",
                columns: new[] { "StartDateUtc", "EndDateUtc", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountPromotionProducts");

            migrationBuilder.DropTable(
                name: "DiscountPromotions");

            migrationBuilder.DropTable(
                name: "PointsPromotions");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
