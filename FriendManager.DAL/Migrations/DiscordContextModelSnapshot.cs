﻿// <auto-generated />
using System;
using FriendManager.DAL.Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FriendManager.DAL.Migrations
{
    [DbContext(typeof(DiscordContext))]
    partial class DiscordContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FriendManager.DAL.Discord.Models.DALDiscordChannel", b =>
                {
                    b.Property<decimal>("ChannelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal?>("ParentChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("SourceChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("SourceChannelName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("SourceGuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("SourceGuildName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal?>("SourceParentChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("ChannelId");

                    b.HasIndex("GuildId");

                    b.HasIndex("ParentChannelId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("FriendManager.DAL.Discord.Models.DALDiscordChannelSyncLog", b =>
                {
                    b.Property<int>("ChannelSyncLogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ChannelSyncLogId"));

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("LastSynchedMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime?>("SynchedDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("ChannelSyncLogId");

                    b.HasIndex("ChannelId");

                    b.ToTable("ChannelSyncLogs");
                });

            modelBuilder.Entity("FriendManager.DAL.Discord.Models.DALDiscordGuild", b =>
                {
                    b.Property<decimal>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("GuildId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("FriendManager.DAL.Discord.Models.DALDiscordChannel", b =>
                {
                    b.HasOne("FriendManager.DAL.Discord.Models.DALDiscordGuild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FriendManager.DAL.Discord.Models.DALDiscordChannel", "ParentChannel")
                        .WithMany()
                        .HasForeignKey("ParentChannelId");

                    b.Navigation("Guild");

                    b.Navigation("ParentChannel");
                });

            modelBuilder.Entity("FriendManager.DAL.Discord.Models.DALDiscordChannelSyncLog", b =>
                {
                    b.HasOne("FriendManager.DAL.Discord.Models.DALDiscordChannel", "Channel")
                        .WithMany("Logs")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");
                });

            modelBuilder.Entity("FriendManager.DAL.Discord.Models.DALDiscordChannel", b =>
                {
                    b.Navigation("Logs");
                });
#pragma warning restore 612, 618
        }
    }
}
