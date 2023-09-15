using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.BAL.Discord
{
    public class DiscordRoleConfig
    {
        public ulong RoleId { get; set; }
        public string RoleName { get; set; }
        public int NumKeys { get; set; }
    }
}
