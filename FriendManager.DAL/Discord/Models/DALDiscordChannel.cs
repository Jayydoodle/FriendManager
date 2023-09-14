using FriendManager.DAL.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.DAL.Discord.Models
{
    public class DALDiscordChannel : DALObjectBase
    {
        [Key]
        public ulong ChannelId { get; set; }
        public string Name { get; set; }

        public ulong GuildId { get; set; }
        public virtual DALDiscordGuild Guild { get; set; }

        public ulong? ParentChannelId { get; set; }
        public virtual DALDiscordChannel ParentChannel { get; set; }

        public ICollection<DALDiscordChannelSyncLog> Logs { get; set; }

        public ulong SourceGuildId { get; set; }
        public string SourceGuildName { get; set; }
        public ulong SourceChannelId { get; set; }
        public string SourceChannelName { get; set; }
        public ulong? SourceParentChannelId { get; set; }

    }
}
