using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IHM_Distribution.Migrations
{
    /// <inheritdoc />
    public partial class updateDailyTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "DailyTrips",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "DailyTrips");
        }
    }
}
