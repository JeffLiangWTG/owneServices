using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Migrations
{
	public partial class AddGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "MessageEvent",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MessageFlowGuid",
                table: "MessageEvent",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ConversationGuid",
                table: "Flows",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Flows",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Conversations",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.Sql("UPDATE Conversations SET Guid = NEWID()");
			migrationBuilder.Sql("UPDATE Flows SET Guid = NEWID()");
			migrationBuilder.Sql("UPDATE MessageEvent SET Guid = NEWID()");

			migrationBuilder.AddUniqueConstraint(
                name: "AK_MessageEvent_Guid",
                table: "MessageEvent",
                column: "Guid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Flows_Guid",
                table: "Flows",
                column: "Guid");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Conversations_Guid",
                table: "Conversations",
                column: "Guid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_MessageEvent_Guid",
                table: "MessageEvent");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Flows_Guid",
                table: "Flows");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Conversations_Guid",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "MessageEvent");

            migrationBuilder.DropColumn(
                name: "MessageFlowGuid",
                table: "MessageEvent");

            migrationBuilder.DropColumn(
                name: "ConversationGuid",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Conversations");
        }
    }
}
