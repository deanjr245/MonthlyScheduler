using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailySchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GeneratedScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailySchedules_GeneratedSchedules_GeneratedScheduleId",
                        column: x => x.GeneratedScheduleId,
                        principalTable: "GeneratedSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    DutyTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleAssignments_DailySchedules_DailyScheduleId",
                        column: x => x.DailyScheduleId,
                        principalTable: "DailySchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleAssignments_DutyTypes_DutyTypeId",
                        column: x => x.DutyTypeId,
                        principalTable: "DutyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleAssignments_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailySchedules_GeneratedScheduleId",
                table: "DailySchedules",
                column: "GeneratedScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedSchedules_Year_Month_IsActive",
                table: "GeneratedSchedules",
                columns: new[] { "Year", "Month", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignments_DailyScheduleId",
                table: "ScheduleAssignments",
                column: "DailyScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignments_DutyTypeId",
                table: "ScheduleAssignments",
                column: "DutyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignments_MemberId",
                table: "ScheduleAssignments",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleAssignments");

            migrationBuilder.DropTable(
                name: "DailySchedules");

            migrationBuilder.DropTable(
                name: "GeneratedSchedules");
        }
    }
}
