using Microsoft.EntityFrameworkCore.Migrations;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Migrations
{
    public partial class MakeEnumColumnsStrings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Flows",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Conversations",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Flows",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Conversations",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
