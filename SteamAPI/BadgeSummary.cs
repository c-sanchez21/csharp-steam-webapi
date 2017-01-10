using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class BadgeSummary
    {
        public Badge[] badges { get; set; }
        public int player_xp { get; set; }
        public int player_level { get; set; }
        public int player_xp_needed_to_level_up { get; set; }
        public int player_xp_needed_current_level { get; set; }

        public class Badge
        {
            public int badgeid { get; set; }
            public int level { get; set; }
            public int completion_time { get; set; }
            public int xp { get; set; }
            public int scarcity { get; set; }
            public ulong appid { get; set; }
            public ulong communityitemid { get; set; }
            public int border_color { get; set; }
        }
    }
}
