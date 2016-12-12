using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class OwnedGame
    {
        /// <summary>
        /// Unique identifier for the game
        /// </summary>
        public ulong AppID { get; set; }

        /// <summary>
        /// The name of the game 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total number of minutes played in the last 2 weeks 
        /// </summary>
        public ulong Playtime2Weeks { get; set; }

        /// <summary>
        /// The total number of minutes played "on record", since Steam began tracking total playtime in early 2009. 
        /// </summary>
        public ulong PlaytimeForever { get; set; }

        /// <summary>
        /// these are the filenames of various images for the game. 
        /// To construct the URL to the image, use this format: http://media.steampowered.com/steamcommunity/public/images/apps/{appid}/{hash}.jpg. 
        /// For example, the TF2 logo is returned as "07385eb55b5ba974aebbe74d3c99626bda7920b8", which maps to the URL:
        /// </summary>
        public string ImageIconUrl { get; set; }
        /// <summary>
        /// these are the filenames of various images for the game.
        /// To construct the URL to the image, use this format: http://media.steampowered.com/steamcommunity/public/images/apps/{appid}/{hash}.jpg. 
        /// For example, the TF2 logo is returned as "07385eb55b5ba974aebbe74d3c99626bda7920b8", which maps to the URL:
        /// </summary>
        public string ImageLogoUrl { get; set; }

        /// <summary>
        /// indicates there is a stats page with achievements or other game stats available for this game. 
        /// The uniform URL for accessing this data is http://steamcommunity.com/profiles/{steamid}/stats/{appid}. 
        /// For example, Robin's TF2 stats can be found at: http://steamcommunity.com/profiles/76561197960435530/stats/440. 
        /// You may notice that clicking this link will actually redirect to a vanity URL like /id/robinwalker/stats/TF2 
        /// </summary>
        public bool HasCommunityVisibleStats { get; set; }
    }
}
