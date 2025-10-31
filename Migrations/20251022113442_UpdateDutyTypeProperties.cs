using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDutyTypeProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDefaultWednesday",
                table: "DutyTypes",
                newName: "IsWednesdayDuty");

            migrationBuilder.RenameColumn(
                name: "IsDefaultMorning",
                table: "DutyTypes",
                newName: "IsMorningDuty");

            migrationBuilder.RenameColumn(
                name: "IsDefaultEvening",
                table: "DutyTypes",
                newName: "IsEveningDuty");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsWednesdayDuty",
                table: "DutyTypes",
                newName: "IsDefaultWednesday");

            migrationBuilder.RenameColumn(
                name: "IsMorningDuty",
                table: "DutyTypes",
                newName: "IsDefaultMorning");

            migrationBuilder.RenameColumn(
                name: "IsEveningDuty",
                table: "DutyTypes",
                newName: "IsDefaultEvening");
        }
    }
}
