using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseUtcDateTimePromotions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "PointsPromotions",
                newName: "StartDateUtc");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "PointsPromotions",
                newName: "EndDateUtc");

            migrationBuilder.RenameIndex(
                name: "IX_PointsPromotions_StartDate_EndDate_Category",
                table: "PointsPromotions",
                newName: "IX_PointsPromotions_StartDateUtc_EndDateUtc_Category");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "DiscountPromotions",
                newName: "StartDateUtc");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "DiscountPromotions",
                newName: "EndDateUtc");

            migrationBuilder.RenameIndex(
                name: "IX_DiscountPromotions_StartDate_EndDate",
                table: "DiscountPromotions",
                newName: "IX_DiscountPromotions_StartDateUtc_EndDateUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDateUtc",
                table: "PointsPromotions",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndDateUtc",
                table: "PointsPromotions",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_PointsPromotions_StartDateUtc_EndDateUtc_Category",
                table: "PointsPromotions",
                newName: "IX_PointsPromotions_StartDate_EndDate_Category");

            migrationBuilder.RenameColumn(
                name: "StartDateUtc",
                table: "DiscountPromotions",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndDateUtc",
                table: "DiscountPromotions",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_DiscountPromotions_StartDateUtc_EndDateUtc",
                table: "DiscountPromotions",
                newName: "IX_DiscountPromotions_StartDate_EndDate");
        }
    }
}
