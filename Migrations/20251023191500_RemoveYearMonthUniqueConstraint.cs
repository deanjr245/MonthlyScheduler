using Microsoft.EntityFrameworkCore.Migrations;

using MonthlyScheduler.Data;

namespace MonthlyScheduler.Migrations;

public partial class RemoveYearMonthUniqueConstraint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_GeneratedSchedules_Year_Month",
            table: "GeneratedSchedules");

        // Re-create the index without the unique constraint
        migrationBuilder.CreateIndex(
            name: "IX_GeneratedSchedules_Year_Month",
            table: "GeneratedSchedules",
            columns: new[] { "Year", "Month" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_GeneratedSchedules_Year_Month",
            table: "GeneratedSchedules");

        // Re-create the unique index
        migrationBuilder.CreateIndex(
            name: "IX_GeneratedSchedules_Year_Month",
            table: "GeneratedSchedules",
            columns: new[] { "Year", "Month" },
            unique: true);
    }
}