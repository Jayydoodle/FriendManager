using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.Discord
{
    public abstract class DiscordClientBase
    {
        #region Properties

        public bool Initialized { get; set; }
        public bool Locked { get; set; }
        public ulong LogChannelId { get; set; }

        #endregion

        #region Public API

        public virtual async Task Initialize()
        {
            Initialized = true;
            await Task.CompletedTask;
        }

        public virtual async Task Shutdown()
        {
            Initialized = false;
            await Task.CompletedTask;
        }

        #endregion
    }
}
