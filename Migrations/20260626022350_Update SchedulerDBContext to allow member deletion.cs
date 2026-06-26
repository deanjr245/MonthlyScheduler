using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchedulerDBContexttoallowmemberdeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleAssignments_Members_MemberId",
                table: "ScheduleAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceType",
                table: "ScheduleAssignments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleAssignments_Members_MemberId",
                table: "ScheduleAssignments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleAssignments_Members_MemberId",
                table: "ScheduleAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceType",
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
                principalColumn: "Id");
        }
    }
}
