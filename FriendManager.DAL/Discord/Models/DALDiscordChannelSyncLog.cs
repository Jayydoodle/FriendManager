using FriendManager.DAL.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.DAL.Discord.Models
{
    public class DALDiscordChannelSyncLog : DALObjectBase
    {
        [Key]
        public int ChannelSyncLogId { get; set; }
        public ulong? LastSynchedMessageId { get; set; }
        public DateTime? SynchedDate { get; set; }

        public ulong ChannelId { get; set; }
        public virtual DALDiscordChannel Channel { get; set; }
    }
}
