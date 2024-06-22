using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteRecorder.Migrations
{
    /// <inheritdoc />
    public partial class addedAvgSpeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AvgSpeed",
                table: "Routes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgSpeed",
                table: "Routes");
        }
    }
}
