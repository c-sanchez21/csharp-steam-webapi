using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class Badge
    {
        public int BadgeID { get; set; }
        public ulong AppID { get; set; }
        public int Level { get; set; }
        public DateTime CompletionTime { get; set; }
        public int XP { get; set; }
        public ulong CommunityItemID { get; set; }
        public int BorderColor { get; set; }
        public ulong Scarcity { get; set; }
    }
}
