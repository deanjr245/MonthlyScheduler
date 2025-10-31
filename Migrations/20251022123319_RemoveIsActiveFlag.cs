using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsActiveFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeneratedSchedules_Year_Month_IsActive",
                table: "GeneratedSchedules");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GeneratedSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedSchedules_Year_Month",
                table: "GeneratedSchedules",
                columns: new[] { "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeneratedSchedules_Year_Month",
                table: "GeneratedSchedules");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GeneratedSchedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedSchedules_Year_Month_IsActive",
                table: "GeneratedSchedules",
                columns: new[] { "Year", "Month", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");
        }
    }
}
