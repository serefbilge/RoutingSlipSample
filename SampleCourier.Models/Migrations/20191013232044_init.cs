using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleCourier.Models.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EfCoreRoutingSlipState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(nullable: false),
                    State = table.Column<string>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: true),
                    EndTime = table.Column<DateTime>(nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: true),
                    FaultSummary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EfCoreRoutingSlipState", x => x.CorrelationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EfCoreRoutingSlipState");
        }
    }
}
