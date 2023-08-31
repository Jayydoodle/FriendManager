using CustomSpectreConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace TGFriendTracker.Functions
{
    public abstract class ManagerBase : ConsoleFunction
    {
        #region Constants

        protected const string FriendActivityEndpoint = "https://prod-api.kosetto.com/friends-activity/";
        public const string HolderDetailsEndpoint = "https://prod-api.kosetto.com/users/{0}/token/holders";

        #endregion

        #region Properties

        public HttpClient WebClient { get; set; }
        public abstract override string DisplayName { get; }

        #endregion
    }

    public abstract class ManagerBase<T> : ManagerBase
    where T : class, new()
    {
        #region Properties

        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());
        public static T Instance => _instance.Value;

        #endregion
    }
}
