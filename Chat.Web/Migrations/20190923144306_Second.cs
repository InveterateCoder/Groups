using Microsoft.EntityFrameworkCore.Migrations;

namespace Chat.Web.Migrations
{
    public partial class Second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscriptionJson",
                table: "Chatterers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionJson",
                table: "Chatterers");
        }
    }
}
