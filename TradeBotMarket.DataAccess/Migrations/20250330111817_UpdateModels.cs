using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeBotMarket.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FuturePrices_Symbol_Timestamp",
                table: "FuturePrices");

            migrationBuilder.CreateIndex(
                name: "IX_FuturePrices_Symbol_Timestamp",
                table: "FuturePrices",
                columns: new[] { "Symbol", "Timestamp" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FuturePrices_Symbol_Timestamp",
                table: "FuturePrices");

            migrationBuilder.CreateIndex(
                name: "IX_FuturePrices_Symbol_Timestamp",
                table: "FuturePrices",
                columns: new[] { "Symbol", "Timestamp" });
        }
    }
}
