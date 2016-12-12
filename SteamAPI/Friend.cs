using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class Friend
    {
        public ulong SteamID { get; set; }
        public DateTime FriendSince { get; set; }

        public Friend(ulong steamID, DateTime friendSince)
        {
            this.SteamID = steamID;
            this.FriendSince = friendSince;
        }
    }
}
