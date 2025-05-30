using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryX.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadedBookFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThumbnailUrl",
                table: "DownloadedBooks",
                newName: "CoverImageUrl");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Format",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookUrl",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookUrl",
                table: "DownloadedBooks");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "DownloadedBooks");

            migrationBuilder.RenameColumn(
                name: "CoverImageUrl",
                table: "DownloadedBooks",
                newName: "ThumbnailUrl");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Format",
                table: "DownloadedBooks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);
        }
    }
}
