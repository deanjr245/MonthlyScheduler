using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyScheduler.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceOrderIndexWithServiceSpecific : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderIndex",
                table: "DutyTypes",
                newName: "OrderIndexWednesday");

            migrationBuilder.AddColumn<int>(
                name: "OrderIndexAM",
                table: "DutyTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndexPM",
                table: "DutyTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderIndexAM",
                table: "DutyTypes");

            migrationBuilder.DropColumn(
                name: "OrderIndexPM",
                table: "DutyTypes");

            migrationBuilder.RenameColumn(
                name: "OrderIndexWednesday",
                table: "DutyTypes",
                newName: "OrderIndex");
        }
    }
}
