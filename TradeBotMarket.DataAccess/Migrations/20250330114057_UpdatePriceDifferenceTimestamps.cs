using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeBotMarket.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePriceDifferenceTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceDifferences_FirstSymbol_SecondSymbol_Timestamp",
                table: "PriceDifferences");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "PriceDifferences",
                newName: "SecondPriceTimestamp");

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstPriceTimestamp",
                table: "PriceDifferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_PriceDifferences_FirstSymbol_SecondSymbol_FirstPriceTimesta~",
                table: "PriceDifferences",
                columns: new[] { "FirstSymbol", "SecondSymbol", "FirstPriceTimestamp", "SecondPriceTimestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceDifferences_FirstSymbol_SecondSymbol_FirstPriceTimesta~",
                table: "PriceDifferences");

            migrationBuilder.DropColumn(
                name: "FirstPriceTimestamp",
                table: "PriceDifferences");

            migrationBuilder.RenameColumn(
                name: "SecondPriceTimestamp",
                table: "PriceDifferences",
                newName: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PriceDifferences_FirstSymbol_SecondSymbol_Timestamp",
                table: "PriceDifferences",
                columns: new[] { "FirstSymbol", "SecondSymbol", "Timestamp" });
        }
    }
}
