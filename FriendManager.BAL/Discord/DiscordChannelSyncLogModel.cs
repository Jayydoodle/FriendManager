using FriendManager.DAL.Discord.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.BAL.Discord
{
    public class DiscordChannelSyncLogModel : DiscordObjectBase<DiscordChannelSyncLogModel, DALDiscordChannelSyncLog>
    {
        #region Properties

        public int Id
        {
            get { return _item.ChannelSyncLogId; }
            set { _item.ChannelSyncLogId = value; }
        }
        public ulong? LastSynchedMessageId
        {
            get { return _item.LastSynchedMessageId; }
            set { _item.LastSynchedMessageId = value; }
        }
        public DateTime? SynchedDate
        {
            get { return _item.SynchedDate; }
            set { _item.SynchedDate = value; }
        }

        public ulong ChannelId
        {
            get { return _item.ChannelId; }
            set { _item.ChannelId = value; }
        }

        private DiscordChannelModel _channel;
        public DiscordChannelModel Channel
        {
            get
            {
                if (_channel == null)
                {
                    if (_item.ChannelId != null)
                        _channel = new DiscordChannelModel(_item.Channel);
                    else if (ChannelId > 0)
                        _channel = new DiscordChannelModel(ChannelId);
                }

                return _channel;
            }
        }

        #endregion

        #region Life Cycle

        public DiscordChannelSyncLogModel()
        : base() { }

        public DiscordChannelSyncLogModel(object primaryKeyValue)
        : base(primaryKeyValue) { }

        public DiscordChannelSyncLogModel(DALDiscordChannelSyncLog item)
        : base(item) { }

        #endregion
    }
}
