using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserAuthFunctionality.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NewChangesIntoToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "refreshTokens",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Expires",
                table: "refreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "refreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpired",
                table: "refreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_refreshTokens_Token",
                table: "refreshTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refreshTokens_Token",
                table: "refreshTokens");

            migrationBuilder.DropColumn(
                name: "Expires",
                table: "refreshTokens");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "refreshTokens");

            migrationBuilder.DropColumn(
                name: "IsExpired",
                table: "refreshTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "refreshTokens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);
        }
    }
}
