using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.BAL.Discord
{
    public class DiscordEmbedDTO
    {
        public string AuthorName { get; set; }
        public string AuthorIconUrl { get; set; }
        public string AuthorIconProxyUrl { get; set; }
        public string FooterText { get; set; }
        public string FooterIconUrl { get; set; }
        public string FooterIconProxyUrl { get; set; }
        public List<DiscordEmbedFieldDTO> Fields { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? TimeStamp { get; set; }

        public DiscordEmbedDTO() 
        {
            Fields = new List<DiscordEmbedFieldDTO>();
        }

    }

    public class DiscordEmbedFieldDTO
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
