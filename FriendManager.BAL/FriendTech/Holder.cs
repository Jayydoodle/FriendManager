using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FriendManager.BAL.FriendTech;

namespace FriendManager.BAL
{
    namespace FriendTechTracker.BAL
    {
        public class FriendTechHolderDetails
        {
            public List<Holder> Users { get; set; }
            public int? NextPageStart { get; set; }
        }

        public class Holder
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("twitterUsername")]
            public string TwitterUsername { get; set; }

            [JsonProperty("twitterName")]
            public string TwitterName { get; set; }

            [JsonProperty("twitterPfpUrl")]
            public string TwitterPfpUrl { get; set; }

            [JsonProperty("twitterUserId")]
            public string TwitterUserId { get; set; }

            [JsonProperty("lastOnline")]
            public long LastOnline { get; set; }

            [JsonProperty("balance")]
            public string Balance { get; set; }

            [JsonIgnore]
            public HolderUserMapping UserMapping { get; set; }
        }
    }

}
