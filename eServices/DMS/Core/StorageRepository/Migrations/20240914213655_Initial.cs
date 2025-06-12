using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eServices.Dms.Core.StorageRepository.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DmsStorageCatalog",
                columns: table => new
                {
                    SC_Schema = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SC_Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SC_Type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SC_Version = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SC_VersionMajor = table.Column<int>(type: "int", nullable: false, computedColumnSql: "(convert([int],left([SC_Version],charindex('.',[SC_Version])-(1))))", stored: false),
                    SC_VersionMinor = table.Column<int>(type: "int", nullable: false, computedColumnSql: "(convert([int],substring([SC_Version],charindex('.',[SC_Version])+(1),case when charindex('-',[SC_Version])>(0) then charindex('-',[SC_Version])-(1) else len([SC_Version]) end-charindex('.',[SC_Version]))))", stored: false),
                    SC_VersionLabel = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, computedColumnSql: "(substring([SC_Version],charindex('-',[SC_Version]),case when charindex('-',[SC_Version])>(0) then (len([SC_Version])-charindex('-',[SC_Version]))+(1) else (0) end))", stored: false),
                    SC_ExpirationDays = table.Column<int>(type: "int", nullable: true),
                    SC_CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    SC_CreateUser = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false, defaultValueSql: "(suser_name())"),
                    SC_DeleteTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SC_DeleteUser = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmsStorageCatalog", x => new { x.SC_Schema, x.SC_Name });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DmsStorageCatalog");
        }
    }
}
