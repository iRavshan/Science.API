using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Science.Data.Migrations
{
    /// <inheritdoc />
    public partial class nextMigration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAdress",
                table: "UserAgents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAdress",
                table: "UserAgents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
