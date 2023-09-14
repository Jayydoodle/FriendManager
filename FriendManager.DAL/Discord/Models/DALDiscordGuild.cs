using FriendManager.DAL.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.DAL.Discord.Models
{
    public class DALDiscordGuild : DALObjectBase
    {
        [Key]
        public ulong GuildId { get; set; }
        public string Name { get; set; }
    }
}
