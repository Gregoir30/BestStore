using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BestStore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMiagrationCommentaireEmoji : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Comments");
        }
    }
}
