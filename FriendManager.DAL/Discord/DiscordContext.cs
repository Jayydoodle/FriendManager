using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendManager.DAL.Discord.Models;
using FriendManager.DAL.Base;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CustomSpectreConsole.Settings;

namespace FriendManager.DAL.Discord
{
    public class DiscordContext : ContextBase
    {
        #region Instances

        public DbSet<DALDiscordGuild> Guilds { get; set; }
        public DbSet<DALDiscordChannel> Channels { get; set; }
        public DbSet<DALDiscordChannelSyncLog> ChannelSyncLogs { get; set; }

        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(XMLSettings.GetValue(ConnectionStrings.Discord));

    }
}
