using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SymphonyLimited.Migrations
{
    /// <inheritdoc />
    public partial class ExtendAboutInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Batches",
                table: "AboutInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Faculty",
                table: "AboutInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstitutionImagePath",
                table: "AboutInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mission",
                table: "AboutInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Vision",
                table: "AboutInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Batches",
                table: "AboutInfos");

            migrationBuilder.DropColumn(
                name: "Faculty",
                table: "AboutInfos");

            migrationBuilder.DropColumn(
                name: "InstitutionImagePath",
                table: "AboutInfos");

            migrationBuilder.DropColumn(
                name: "Mission",
                table: "AboutInfos");

            migrationBuilder.DropColumn(
                name: "Vision",
                table: "AboutInfos");
        }
    }
}
