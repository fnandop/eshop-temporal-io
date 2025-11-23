using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOrderyGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderyGuid",
                schema: "ordering",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_orders_OrderyGuid",
                schema: "ordering",
                table: "orders",
                column: "OrderyGuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_OrderyGuid",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "OrderyGuid",
                schema: "ordering",
                table: "orders");
        }
    }
}
