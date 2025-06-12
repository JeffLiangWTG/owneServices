using Microsoft.EntityFrameworkCore.Migrations;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Migrations
{
    public partial class AddEventDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Detail",
                table: "MessageEvent",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Detail",
                table: "MessageEvent");
        }
    }
}
