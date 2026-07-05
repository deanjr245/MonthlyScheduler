using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignmentCategoryId",
                table: "ScheduleAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignmentCategoryId",
                table: "DutyTypes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignmentCategoryId",
                table: "DutyAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssignmentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    MaxAssignmentsPerMonth = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignments_AssignmentCategoryId",
                table: "ScheduleAssignments",
                column: "AssignmentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DutyTypes_AssignmentCategoryId",
                table: "DutyTypes",
                column: "AssignmentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DutyAssignments_AssignmentCategoryId",
                table: "DutyAssignments",
                column: "AssignmentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DutyAssignments_AssignmentCategories_AssignmentCategoryId",
                table: "DutyAssignments",
                column: "AssignmentCategoryId",
                principalTable: "AssignmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DutyTypes_AssignmentCategories_AssignmentCategoryId",
                table: "DutyTypes",
                column: "AssignmentCategoryId",
                principalTable: "AssignmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleAssignments_AssignmentCategories_AssignmentCategoryId",
                table: "ScheduleAssignments",
                column: "AssignmentCategoryId",
                principalTable: "AssignmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.InsertData(
                table: "AssignmentCategories",
                columns: new[] { "Id", "Name", "Description", "MaxAssignmentsPerMonth" },
                values: new object[,] {
                    { 1, "Song Leading", "Song leading roles across Sunday and Wednesday services", 1 },
                    { 2, "Prayer", "Opening and closing prayer responsibilities", 1 },
                    { 3, "Table Presiding", "Lord's supper presiding assignments", 1 },
                });

            migrationBuilder.Sql(@"
                UPDATE DutyTypes
                SET AssignmentCategoryId = CASE
                    WHEN Name LIKE '%Song%' THEN 1
                    WHEN Name LIKE '%Prayer%' THEN 2
                    WHEN Name LIKE '%Preside%' THEN 3
                    ELSE NULL
                END
                WHERE AssignmentCategoryId IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DutyAssignments_AssignmentCategories_AssignmentCategoryId",
                table: "DutyAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyTypes_AssignmentCategories_AssignmentCategoryId",
                table: "DutyTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleAssignments_AssignmentCategories_AssignmentCategoryId",
                table: "ScheduleAssignments");

            migrationBuilder.DropTable(
                name: "AssignmentCategories");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleAssignments_AssignmentCategoryId",
                table: "ScheduleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_DutyTypes_AssignmentCategoryId",
                table: "DutyTypes");

            migrationBuilder.DropIndex(
                name: "IX_DutyAssignments_AssignmentCategoryId",
                table: "DutyAssignments");

            migrationBuilder.DropColumn(
                name: "AssignmentCategoryId",
                table: "ScheduleAssignments");

            migrationBuilder.DropColumn(
                name: "AssignmentCategoryId",
                table: "DutyTypes");

            migrationBuilder.DropColumn(
                name: "AssignmentCategoryId",
                table: "DutyAssignments");
        }
    }
}
