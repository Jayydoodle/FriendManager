using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FriendManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _091023_0223am : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastSynchedDate",
                table: "Channels",
                newName: "SynchedDate");

            migrationBuilder.CreateTable(
                name: "ChannelSyncLogs",
                columns: table => new
                {
                    ChannelSyncLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastSynchedMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    SynchedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SourceGuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SourceChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSyncLogs", x => x.ChannelSyncLogId);
                    table.ForeignKey(
                        name: "FK_ChannelSyncLogs_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSyncLogs_ChannelId",
                table: "ChannelSyncLogs",
                column: "ChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelSyncLogs");

            migrationBuilder.RenameColumn(
                name: "SynchedDate",
                table: "Channels",
                newName: "LastSynchedDate");
        }
    }
}
