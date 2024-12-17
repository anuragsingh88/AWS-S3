using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWS_S3.Migrations
{
    /// <inheritdoc />
    public partial class awsconfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.CreateTable(
                name: "AWSConfigurations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Region = table.Column<string>(type: "TEXT", nullable: false),
                    Bucket = table.Column<string>(type: "TEXT", nullable: false),
                    AccessKeyID = table.Column<string>(type: "TEXT", nullable: false),
                    SecretAccessKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AWSConfigurations", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AWSConfigurations");

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessKeyID = table.Column<string>(type: "TEXT", nullable: false),
                    Bucket = table.Column<string>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", nullable: false),
                    SecretAccessKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.ID);
                });
        }
    }
}
