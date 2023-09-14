using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FriendManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _091423_154pm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ParentChannelId",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SourceParentChannelId",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ParentChannelId",
                table: "Channels",
                column: "ParentChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Channels_ParentChannelId",
                table: "Channels",
                column: "ParentChannelId",
                principalTable: "Channels",
                principalColumn: "ChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Channels_ParentChannelId",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Channels_ParentChannelId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "ParentChannelId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "SourceParentChannelId",
                table: "Channels");
        }
    }
}
