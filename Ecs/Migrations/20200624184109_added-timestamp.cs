using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecs.Migrations
{
    public partial class addedtimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeStamp",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    In = table.Column<DateTime>(nullable: false),
                    InWhileOnPremises = table.Column<bool>(nullable: false),
                    Out = table.Column<DateTime>(nullable: true),
                    OutWhileOnPremises = table.Column<bool>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeStamp", x => new { x.EmployeeId, x.Id });
                    table.ForeignKey(
                        name: "FK_TimeStamp_AspNetUsers_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeStamp");
        }
    }
}
