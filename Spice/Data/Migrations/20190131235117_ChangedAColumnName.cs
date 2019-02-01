using Microsoft.EntityFrameworkCore.Migrations;

namespace Spice.Data.Migrations
{
    public partial class ChangedAColumnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderTotalDiscounted",
                table: "Orders",
                newName: "OrderTotalOriginal");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderTotalOriginal",
                table: "Orders",
                newName: "OrderTotalDiscounted");
        }
    }
}
