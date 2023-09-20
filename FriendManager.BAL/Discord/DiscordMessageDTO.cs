using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.BAL.Discord
{
    public class DiscordMessageDTO
    {
        public ulong MessageId { get; set; }
        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public ulong? ParentChannelId { get; set; }
        public string ParentChannelName { get; set; }
        public string MessageContent { get; set; }
        public DateTime SentAt { get; set; }
        public List<DiscordAttachmentDTO> Attachments { get; set; }
        public DiscordEmbedDTO Embed { get; set; }

        public DiscordMessageDTO() 
        { 
            Attachments = new List<DiscordAttachmentDTO>();
        }
    }
}
