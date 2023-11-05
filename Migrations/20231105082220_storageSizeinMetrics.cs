using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostgreSqlMonitoringBot.Migrations
{
    /// <inheritdoc />
    public partial class storageSizeinMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LowSize",
                table: "Metrics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OffSize",
                table: "Metrics",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LowSize",
                table: "Metrics");

            migrationBuilder.DropColumn(
                name: "OffSize",
                table: "Metrics");
        }
    }
}
