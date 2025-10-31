using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class SeedDutyTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 1, "Scripture Reading", "Read scripture during service", 0, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 2, "AM Song Leading", "Lead songs during Sunday morning service", 0, true, false, false });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 3, "PM Song Leading", "Lead songs during Sunday evening service", 0, false, true, false });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 4, "Wed Song Leading", "Lead songs during Wednesday service", 0, false, false, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 5, "AM Preside at Table", "Preside at the Lord's table (Morning Service)", 0, true, false, false });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 6, "PM Preside at Table", "Preside at the Lord's table (Evening Service)", 0, false, true, false });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 7, "Opening Prayer", "Lead opening prayer", 0, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 8, "Closing Prayer", "Lead closing prayer", 0, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 9, "Foyer Security", "Monitor foyer and entrances", 0, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 10, "Visitor Usher", "Welcome and assist visitors", 0, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 11, "Sound Board Operator", "Operate the sound system", 1, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 12, "Advance Song Slides", "Manage song slides during service", 1, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 13, "AV Booth Operator", "Operate audio/visual equipment", 1, true, true, true });

            migrationBuilder.InsertData(
                table: "DutyTypes",
                columns: new[] { "Id", "Name", "Description", "Category", "IsDefaultMorning", "IsDefaultEvening", "IsDefaultWednesday" },
                values: new object[] { 14, "Transportation", "Assist with transportation needs", 0, true, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DutyTypes");
        }
    }
}
