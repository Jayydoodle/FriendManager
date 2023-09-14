using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FriendManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _091023_0512pm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceChannelId",
                table: "ChannelSyncLogs");

            migrationBuilder.DropColumn(
                name: "SourceGuildId",
                table: "ChannelSyncLogs");

            migrationBuilder.DropColumn(
                name: "LastSynchedMessageId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "SynchedDate",
                table: "Channels");

            migrationBuilder.AddColumn<decimal>(
                name: "SourceChannelId",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SourceChannelName",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SourceGuildId",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SourceGuildName",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceChannelId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "SourceChannelName",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "SourceGuildId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "SourceGuildName",
                table: "Channels");

            migrationBuilder.AddColumn<decimal>(
                name: "SourceChannelId",
                table: "ChannelSyncLogs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SourceGuildId",
                table: "ChannelSyncLogs",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LastSynchedMessageId",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SynchedDate",
                table: "Channels",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
