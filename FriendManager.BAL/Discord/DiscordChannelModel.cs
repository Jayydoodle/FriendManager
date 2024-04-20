using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CustomSpectreConsole;
using FriendManager.DAL.Discord.Models;

namespace FriendManager.BAL.Discord
{
    public class DiscordChannelModel : DiscordObjectBase<DiscordChannelModel, DALDiscordChannel>
    {
        #region Properties

        [TableColumnAttribute]
        public ulong Id
        {
            get { return _item.ChannelId; }
            set { _item.ChannelId = value; }
        }

        [TableColumnAttribute]
        public string Name
        {
            get { return _item.Name; }
            set { _item.Name = value; }
        }

        [TableColumnAttribute]
        public ulong GuildId
        {
            get { return _item.GuildId; }
            set { _item.GuildId = value; }
        }

        private DiscordGuildModel _guild;
        public DiscordGuildModel Guild
        {
            get
            {
                if (_guild == null)
                {
                    if(_item.Guild != null)
                        _guild = new DiscordGuildModel(_item.Guild);
                    else if(GuildId > 0)
                        _guild = new DiscordGuildModel(GuildId);
                }

                return _guild;
            }
        }

        [TableColumnAttribute]
        public ulong? ParentChannelId
        {
            get { return _item.ParentChannelId; }
            set { _item.ParentChannelId = value; }
        }

        private DiscordChannelModel _parentChannel;
        public DiscordChannelModel ParentChannel
        {
            get
            {
                if (_parentChannel == null)
                {
                    if (_item.ParentChannel != null)
                        _parentChannel = new DiscordChannelModel(_item.ParentChannel);
                    else if (ParentChannelId.HasValue)
                        _parentChannel = new DiscordChannelModel(ParentChannelId.Value);
                }

                return _parentChannel;
            }
        }

        private List<DiscordChannelSyncLogModel> _logs;
        public List<DiscordChannelSyncLogModel> Logs
        {
            get
            {
                if (_logs == null)
                {
                    _logs = new List<DiscordChannelSyncLogModel>();

                    if (_item.Logs != null)
                        _logs = _item.Logs.Select(x => new DiscordChannelSyncLogModel(x)).OrderByDescending(x => x.SynchedDate).ToList();
                }

                return _logs;
            }
        }

        public DiscordChannelSyncLogModel LatestLog => Logs.FirstOrDefault();

        [TableColumnAttribute]
        public DateTime CreatedDate
        {
            get { return _item.CreatedDate; }
            set { _item.CreatedDate = value; }
        }

        [TableColumnAttribute]
        public ulong SourceGuildId
        {
            get { return _item.SourceGuildId; }
            set { _item.SourceGuildId = value; }
        }

        [TableColumnAttribute]
        public string SourceGuildName
        {
            get { return _item.SourceGuildName; }
            set { _item.SourceGuildName = value; }
        }

        [TableColumnAttribute]
        public ulong SourceChannelId
        {
            get { return _item.SourceChannelId; }
            set { _item.SourceChannelId = value; }
        }

        [TableColumnAttribute]
        public string SourceChannelName
        {
            get { return _item.SourceChannelName; }
            set { _item.SourceChannelName = value; }
        }

        [TableColumnAttribute]
        public ulong? SourceParentChannelId
        {
            get { return _item.SourceParentChannelId; }
            set { _item.SourceParentChannelId = value; }
        }

        #endregion

        #region Life Cycle

        public DiscordChannelModel()
        : base() { }

        public DiscordChannelModel(object primaryKeyValue)
        : base(primaryKeyValue) { }

        public DiscordChannelModel(DALDiscordChannel item)
        : base(item) { }

        #endregion
    }
}
