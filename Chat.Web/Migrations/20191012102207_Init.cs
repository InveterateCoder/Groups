using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chat.Web.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chatterers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 64, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    Password = table.Column<string>(maxLength: 32, nullable: true),
                    LastActive = table.Column<long>(nullable: false),
                    Group = table.Column<string>(maxLength: 64, nullable: true),
                    GroupLastCleaned = table.Column<long>(nullable: false),
                    GroupPassword = table.Column<string>(maxLength: 32, nullable: true),
                    InGroupId = table.Column<int>(nullable: false),
                    InGroupPassword = table.Column<string>(maxLength: 32, nullable: true),
                    Token = table.Column<string>(maxLength: 50, nullable: true),
                    ConnectionId = table.Column<string>(maxLength: 64, nullable: true),
                    WebSubscription = table.Column<string>(nullable: true),
                    LastNotified = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chatterers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SharpTime = table.Column<long>(nullable: false),
                    JsTime = table.Column<long>(nullable: false),
                    StringTime = table.Column<string>(maxLength: 64, nullable: true),
                    From = table.Column<string>(maxLength: 64, nullable: true),
                    Text = table.Column<string>(maxLength: 10000, nullable: true),
                    GroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chatterers");

            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
