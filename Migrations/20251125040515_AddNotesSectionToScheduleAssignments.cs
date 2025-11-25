using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesSectionToScheduleAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleAssignments_Members_MemberId",
                table: "ScheduleAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "ScheduleAssignments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ScheduleAssignments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleAssignments_Members_MemberId",
                table: "ScheduleAssignments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleAssignments_Members_MemberId",
                table: "ScheduleAssignments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ScheduleAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "ScheduleAssignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleAssignments_Members_MemberId",
                table: "ScheduleAssignments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
