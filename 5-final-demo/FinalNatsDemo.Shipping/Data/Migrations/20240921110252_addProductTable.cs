using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalNatsDemo.Shipping.Data.Migrations
{
    /// <inheritdoc />
    public partial class addProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StockLevel = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(name: "IX_OrderItems_ProductId", table: "OrderItems", column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_OrderItems_Products_ProductId", table: "OrderItems");

            migrationBuilder.DropTable(name: "Products");

            migrationBuilder.DropIndex(name: "IX_OrderItems_ProductId", table: "OrderItems");
        }
    }
}
