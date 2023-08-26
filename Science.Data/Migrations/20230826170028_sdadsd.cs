using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Science.Data.Migrations
{
    /// <inheritdoc />
    public partial class sdadsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISBN",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ImageUrlLarge",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "YearOfPublication",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "ImageUrlSmall",
                table: "Books",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "ImageUrlMedium",
                table: "Books",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "Avilability",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfReviews",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Books",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Stars",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BookId",
                table: "Categories",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropColumn(
                name: "Avilability",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "NumberOfReviews",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Stars",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Books",
                newName: "ImageUrlSmall");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Books",
                newName: "ImageUrlMedium");

            migrationBuilder.AddColumn<string>(
                name: "ISBN",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrlLarge",
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
    }
}
