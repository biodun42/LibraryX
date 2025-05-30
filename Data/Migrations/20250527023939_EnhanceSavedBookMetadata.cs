using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryX.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceSavedBookMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "SavedBooks",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookUrl",
                table: "SavedBooks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categories",
                table: "SavedBooks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ISBN",
                table: "SavedBooks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "SavedBooks",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "SavedBooks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishedYear",
                table: "SavedBooks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "SavedBooks",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingsCount",
                table: "SavedBooks",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "BookUrl",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "Categories",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "ISBN",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "PublishedYear",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "SavedBooks");

            migrationBuilder.DropColumn(
                name: "RatingsCount",
                table: "SavedBooks");
        }
    }
}
