using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS_BlindMatching.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupMembersJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupMembersJson",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupMembersJson",
                table: "Projects");
        }
    }
}
