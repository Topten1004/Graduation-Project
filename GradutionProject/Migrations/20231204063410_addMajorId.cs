using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradutionProject.Migrations
{
    /// <inheritdoc />
    public partial class addMajorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MajorId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MajorId",
                table: "Users");
        }
    }
}
