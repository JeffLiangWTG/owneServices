using Microsoft.EntityFrameworkCore.Migrations;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Migrations
{
    public partial class UpdateConversationColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sender",
                table: "Conversations",
                newName: "Responder");

            migrationBuilder.RenameColumn(
                name: "Recipient",
                table: "Conversations",
                newName: "Initiator");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Responder",
                table: "Conversations",
                newName: "Sender");

            migrationBuilder.RenameColumn(
                name: "Initiator",
                table: "Conversations",
                newName: "Recipient");
        }
    }
}
