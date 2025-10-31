using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DutyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDefaultMorning = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefaultEvening = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefaultWednesday = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DutyTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    HasSubmittedForm = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExcludeFromScheduling = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Service = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberDuties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    DutyTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberDuties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberDuties_DutyTypes_DutyTypeId",
                        column: x => x.DutyTypeId,
                        principalTable: "DutyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberDuties_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DutyAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    DutyTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DutyAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DutyAssignments_DutyTypes_DutyTypeId",
                        column: x => x.DutyTypeId,
                        principalTable: "DutyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DutyAssignments_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DutyAssignments_ServiceSchedules_ServiceScheduleId",
                        column: x => x.ServiceScheduleId,
                        principalTable: "ServiceSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DutyAssignments_DutyTypeId",
                table: "DutyAssignments",
                column: "DutyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DutyAssignments_MemberId",
                table: "DutyAssignments",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_DutyAssignments_ServiceScheduleId",
                table: "DutyAssignments",
                column: "ServiceScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberDuties_DutyTypeId",
                table: "MemberDuties",
                column: "DutyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberDuties_MemberId",
                table: "MemberDuties",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DutyAssignments");

            migrationBuilder.DropTable(
                name: "MemberDuties");

            migrationBuilder.DropTable(
                name: "ServiceSchedules");

            migrationBuilder.DropTable(
                name: "DutyTypes");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
