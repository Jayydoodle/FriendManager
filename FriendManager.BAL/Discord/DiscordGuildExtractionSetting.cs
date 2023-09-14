using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.BAL.Discord
{
    public class DiscordGuildExtractionSetting
    {
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        public List<ulong> ExcludedChannelIds { get; set; }
    }
}
