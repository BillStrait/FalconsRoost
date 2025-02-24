using System;
using FalconsRoost.Models.Alerts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FalconsRoost.Models.DB.Migrations
{
    /// <inheritdoc />
    public partial class Alerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    AlertType = table.Column<int>(type: "int", nullable: false),
                    RecurrenceUnit = table.Column<int>(type: "int", nullable: false),
                    RecurrenceInterval = table.Column<int>(type: "int", nullable: false),
                    DayToRunOn = table.Column<int>(type: "int", nullable: false),
                    HourStartTime = table.Column<int>(type: "int", nullable: false),
                    HourEndTime = table.Column<int>(type: "int", nullable: false),
                    LastRun = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NextRun = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RunOnce = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RunOnStart = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertTasks", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AlertMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    AlertTarget = table.Column<string>(type: "longtext", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: false),
                    AlertTaskId = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertMessages_AlertTasks_AlertTaskId",
                        column: x => x.AlertTaskId,
                        principalTable: "AlertTasks",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AlertMessages_AlertTaskId",
                table: "AlertMessages",
                column: "AlertTaskId");

            // populate the AlertTasks table with some default tasks.
            migrationBuilder.InsertData(
                table: "AlertTasks",
                columns: new[] { "Id", "AlertType", "RecurrenceUnit", "RecurrenceInterval", "DayToRunOn", "HourStartTime", "HourEndTime", "LastRun", "NextRun", "Enabled", "RunOnce", "RunOnStart" },
                values: new object[] { Guid.NewGuid(), 0, 3, 1, 2, 4, 6, DateTime.Now, DateTime.Now, true, false, false });


            migrationBuilder.InsertData(
                table: "AlertTasks",
                columns: new[] { "Id", "AlertType", "RecurrenceUnit", "RecurrenceInterval", "DayToRunOn", "HourStartTime", "HourEndTime", "LastRun", "NextRun", "Enabled", "RunOnce", "RunOnStart" },
                values: new object[] { Guid.NewGuid(), 1, 3, 1, 3, 9, 11, DateTime.Now, DateTime.Now, true, false, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertMessages");

            migrationBuilder.DropTable(
                name: "AlertTasks");
        }
    }
}
