using Microsoft.EntityFrameworkCore.Migrations;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Migrations
{
    public partial class AddComponentToMessageEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageEvent_Flows_MessageFlowId",
                table: "MessageEvent");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "MessageEvent",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MessageFlowId",
                table: "MessageEvent",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Component",
                table: "MessageEvent",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageEvent_Flows_MessageFlowId",
                table: "MessageEvent",
                column: "MessageFlowId",
                principalTable: "Flows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageEvent_Flows_MessageFlowId",
                table: "MessageEvent");

            migrationBuilder.DropColumn(
                name: "Component",
                table: "MessageEvent");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "MessageEvent",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "MessageFlowId",
                table: "MessageEvent",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_MessageEvent_Flows_MessageFlowId",
                table: "MessageEvent",
                column: "MessageFlowId",
                principalTable: "Flows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
