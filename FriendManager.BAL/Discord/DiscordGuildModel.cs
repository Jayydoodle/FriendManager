using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FriendManager.DAL.Discord.Models;

namespace FriendManager.BAL.Discord
{
    public class DiscordGuildModel : DiscordObjectBase<DiscordGuildModel, DALDiscordGuild>
    {
        #region Properties

        public ulong Id
        {
            get { return _item.GuildId; }
            set { _item.GuildId = value; }
        }

        public string Name
        {
            get { return _item.Name; }
            set { _item.Name = value; }
        }

        #endregion

        #region Life Cycle

        public DiscordGuildModel()
        : base() { }

        public DiscordGuildModel(object primaryKeyValue)
        : base(primaryKeyValue) { }

        public DiscordGuildModel(DALDiscordGuild item)
        : base(item) { }

        #endregion
    }
}
