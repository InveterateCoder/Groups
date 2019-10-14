using Microsoft.EntityFrameworkCore.Migrations;

namespace Chat.Web.Migrations
{
    public partial class Fourth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RequestTime",
                table: "RegRequests",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestTime",
                table: "RegRequests");
        }
    }
}
