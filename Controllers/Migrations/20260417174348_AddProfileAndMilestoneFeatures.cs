using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAndMilestoneFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PreferredResearchAreas",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInLink",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubLink",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInLink",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentMilestone",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercentage",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "LinkedInLink",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "GitHubLink",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LinkedInLink",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CurrentMilestone",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "PreferredResearchAreas",
                table: "Supervisors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
