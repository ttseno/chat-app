using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatWSServer.Migrations
{
    public partial class RenameRoomIdentification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatRoom",
                table: "ChatMessages",
                newName: "RoomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "ChatMessages",
                newName: "ChatRoom");
        }
    }
}
