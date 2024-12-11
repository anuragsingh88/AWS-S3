using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWS_S3.Migrations
{
    /// <inheritdoc />
    public partial class columFor2FA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is2FAEnabled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is2FAEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "AspNetUsers");
        }
    }
}
