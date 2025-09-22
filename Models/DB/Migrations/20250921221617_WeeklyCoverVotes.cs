using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FalconsRoost.Models.DB.Migrations
{
    /// <inheritdoc />
    public partial class WeeklyCoverVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoverContest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    WeekEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverContest", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CoverContestCovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    WeeklyContestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LocgId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    IssueNumber = table.Column<string>(type: "longtext", nullable: false),
                    SubmittedByUserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ImagePath = table.Column<string>(type: "longtext", nullable: false),
                    VectorEmbedding = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverContestCovers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverContestCovers_CoverContest_WeeklyContestId",
                        column: x => x.WeeklyContestId,
                        principalTable: "CoverContest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CoverContestVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    WeeklyContestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CoverId = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverContestVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverContestVotes_CoverContestCovers_CoverId",
                        column: x => x.CoverId,
                        principalTable: "CoverContestCovers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoverContestVotes_CoverContest_WeeklyContestId",
                        column: x => x.WeeklyContestId,
                        principalTable: "CoverContest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CoverContestCovers_WeeklyContestId_LocgId",
                table: "CoverContestCovers",
                columns: new[] { "WeeklyContestId", "LocgId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoverContestVotes_CoverId_UserId",
                table: "CoverContestVotes",
                columns: new[] { "CoverId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoverContestVotes_WeeklyContestId",
                table: "CoverContestVotes",
                column: "WeeklyContestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoverContestVotes");

            migrationBuilder.DropTable(
                name: "CoverContestCovers");

            migrationBuilder.DropTable(
                name: "CoverContest");
        }
    }
}
