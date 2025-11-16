using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToTeamMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TeamMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TeamMembers");
        }
    }
}
