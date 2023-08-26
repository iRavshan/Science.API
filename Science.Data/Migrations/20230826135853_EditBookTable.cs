using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Science.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditBookTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrlLarge",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlMedium",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlSmall",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearOfPublication",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrlLarge",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ImageUrlMedium",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ImageUrlSmall",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "YearOfPublication",
                table: "Books");
        }
    }
}
