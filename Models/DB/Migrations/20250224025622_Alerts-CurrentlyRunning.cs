using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FalconsRoost.Models.DB.Migrations
{
    /// <inheritdoc />
    public partial class AlertsCurrentlyRunning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CurrentlyRunning",
                table: "AlertTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentlyRunning",
                table: "AlertTasks");
        }
    }
}
