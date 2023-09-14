using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FriendManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _091023_1225am : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Guilds_GuildId1",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Channels_GuildId1",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "Channels");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_GuildId",
                table: "Channels",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Guilds_GuildId",
                table: "Channels",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Guilds_GuildId",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Channels_GuildId",
                table: "Channels");

            migrationBuilder.AlterColumn<int>(
                name: "GuildId",
                table: "Channels",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<decimal>(
                name: "GuildId1",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_GuildId1",
                table: "Channels",
                column: "GuildId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Guilds_GuildId1",
                table: "Channels",
                column: "GuildId1",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
