using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.BAL.Discord
{
    public class DiscordChannelDTO
    {
        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public ulong? ParentChannelId { get; set; }
    }
}
