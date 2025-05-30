using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace LibraryX.Migrations
{
    public partial class AddDownloadedBookFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add BookUrl column to DownloadedBooks table if it doesn't exist
            migrationBuilder.AddColumn<string>(
                name: "BookUrl",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            // Add Source column as a computed column based on SourceType
            // This ensures backward compatibility without duplicate data
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                computedColumnSql: "SourceType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the columns if the migration is rolled back
            migrationBuilder.DropColumn(
                name: "BookUrl",
                table: "DownloadedBooks");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "DownloadedBooks");
        }
    }
}
