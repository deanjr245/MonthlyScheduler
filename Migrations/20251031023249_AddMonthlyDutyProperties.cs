using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyDutyProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMonthlyDuty",
                table: "DutyTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyDutyFrequency",
                table: "DutyTypes",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMonthlyDuty",
                table: "DutyTypes");

            migrationBuilder.DropColumn(
                name: "MonthlyDutyFrequency",
                table: "DutyTypes");
        }
    }
}
