using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecs.Migrations
{
    public partial class addedtimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Timestamp",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    In = table.Column<DateTime>(nullable: false),
                    InWhileOnPremises = table.Column<bool>(nullable: false),
                    Out = table.Column<DateTime>(nullable: false),
                    OutWhileOnPremises = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timestamp", x => new { x.ApplicationUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_Timestamp_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Timestamp");
        }
    }
}
