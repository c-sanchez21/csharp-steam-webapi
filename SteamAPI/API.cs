using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{    
    public class API
    {
        #region Fields
        private string APIKey;
        const int MaxItemsPerCall = 2000;//The number of items that are shown per call
        #endregion

        /// <summary>
        /// Class constructor for making API calls. 
        /// </summary>
        /// <param name="apiKey">API Key associated with your steam account</param>
        public API(string apiKey)
        {
            this.APIKey = apiKey;
        }

        /// <summary>
        /// Gets the entire Steam App List. 
        /// </summary>
        /// <returns>Returns a list of Apps </returns>
        public List<App> GetAppList()
        {
            List<App> list = new List<App>();
            string json = Request(@"http://api.steampowered.com/ISteamApps/GetAppList/v0001/");
            JObject jObject = JObject.Parse(json);
            JToken apps = jObject["applist"]["apps"];
            JToken[] applist = apps["app"].ToArray<JToken>();
            foreach (JToken app in applist)
                list.Add(new App((ulong)app["appid"], (string)app["name"]));
            return list;
        }

        /// <summary>
        /// Returns the friend list of any Steam user, provided his Steam Community profile visibility is set to "Public". 
        /// </summary>
        /// <param name="steamID">64 bit Steam ID of the friend.</param>
        /// <returns>Returns List of Friend</returns>
        public List<Friend> GetFriendList(ulong steamID)
        {
            List<Friend> list = new List<Friend>();
            string query = @"http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key="+
                APIKey + @"&steamid="+ steamID.ToString()+@"&relationship=friend";

            string jsonString = Request(query);
            JObject jObject = JObject.Parse(jsonString);
            JToken jlist = jObject["friendslist"];                        
            DateTime friendSince;
            ulong friendID;
            foreach (JToken friend in jlist["friends"].ToArray())
            {
                friendID = (ulong)friend["steamid"];
                friendSince = UnixTimeStampToDateTime((double)friend["friend_since"]);
                list.Add(new Friend(friendID, friendSince));
            }
            return list;
        }

        /// <summary>
        /// Returns basic profile information for a single 64-bit Steam ID.
        /// </summary>
        /// <param name="steamID">64 bit Steam ID</param>
        /// <returns>Returns a single PlayerSummary</returns>
        public PlayerSummary GetPlayerSummary(ulong steamID)
        {
            string query = @"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + APIKey + @"&steamids="+steamID.ToString();
            List<JToken> list = new List<JToken>();
            list.AddRange(GetPlayerTokens(Request(query)));
            List<PlayerSummary> summaries = ExtractPlayerSummaries(list);
            if (summaries == null || summaries.Count == 0) return null;
            return summaries[0];            
        }

        /// <summary>
        /// Returns basic profile information for a list of 64-bit Steam IDs.
        /// </summary>
        /// <param name="friends">Can pass in the list from GetFriendList</param>
        /// <returns>Returns a List of PlayerSummary</returns>
        public List<PlayerSummary> GetPlayerSummaries(IEnumerable<Friend> friends)
        {
            string query = @"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key="+APIKey+@"&steamids=";
            StringBuilder sb = new StringBuilder(query);
            int count = 0;
            List<JToken> players = new List<JToken>();
            foreach (Friend f in friends)
            {
                sb.Append(f.SteamID.ToString());
                sb.Append('+');
                count++;
                if (count == 100) //API Limits 100 SteamID's per call
                {
                    sb.Remove(sb.Length - 1, 1);//Remove Last '+'
                    players.AddRange(GetPlayerTokens(Request(sb.ToString()))); //Add the Player JTokens
                    count = 0;
                    sb = new StringBuilder(query);
                }
            }
            if (count > 0)
            {
                sb.Remove(sb.Length - 1, 1);//Remove Last '+'
                players.AddRange(GetPlayerTokens(Request(sb.ToString())));
            }
            return ExtractPlayerSummaries(players);            
        }

        /// <summary>
        /// GetOwnedGames returns a list of games a player owns along with some playtime information, if the profile is publicly visible.
        /// </summary>
        /// <param name="steamID">64-bit Steam ID</param>
        /// <returns>Returns List of OwnedGame</returns>
        public List<OwnedGame> GetOwnedGames(ulong steamID)
        {
            string query = @"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key=" + APIKey +@"&steamid="+ steamID.ToString() + @"&include_appinfo=1";
            string json = Request(query);
            List<OwnedGame> list = new List<OwnedGame>();
            JObject jObject = JObject.Parse(json);
            if (jObject == null) return null;
            JToken response = jObject["response"];
            if (response["games"] == null) return list;
            JToken[] games = response["games"].ToArray<JToken>();
            OwnedGame g;
            foreach(JToken game in games)
            {
                g = new OwnedGame();
                g.AppID = (ulong)game["appid"];
                g.Name = (string)game["name"];
                g.PlaytimeForever = (ulong)game["playtime_forever"];
                g.ImageIconUrl = (string)game["img_icon_url"];
                g.ImageLogoUrl = (string)game["img_logo_url"];
                if (game["has_community_visible_stats"] != null)
                    g.HasCommunityVisibleStats = (bool)game["has_community_visible_stats"];
                list.Add(g);
            }
            return list;            
        }

        /// <summary>
        /// Gets the Steam level of a specified user.
        /// </summary>
        /// <param name="steamID">64-bit Steam ID</param>
        /// <returns>Returns an int denoting user level.</returns>
        public int GetLevel(ulong steamID)
        {
            string query = @"http://api.steampowered.com/IPlayerService/GetSteamLevel/v1?key=" + APIKey + @"&steamid=" + steamID;            
            string json = Request(query);
            JObject jObj = JObject.Parse(json);
            if (jObj == null) return -1;
            JToken response = jObj["response"];
            if (response == null) return -1;
            if (response["player_level"] == null) return -1;
            return (int)response["player_level"];        
    }

        /// <summary>
        /// Get a list of bagde information for specified user.
        /// </summary>
        /// <param name="steamID">64-bit Steam ID</param>
        /// <returns>Returns a list of Badge</returns>
        public List<Badge> GetBadges(ulong steamID)
        {
            List<Badge> list = new List<Badge>();
            string query = @"http://api.steampowered.com/IPlayerService/GetBadges/v1?key=" + APIKey + @"&steamid=" + steamID.ToString();
            string json = Request(query);
            JObject obj = JObject.Parse(json);
            if (obj == null) return list;
            if (obj["response"] == null) return list;
            JToken response = obj["response"];
            if (response["badges"] == null) return list;
            JToken[] badgesArray = response["badges"].ToArray<JToken>();
            Badge b;
            foreach (JToken t in badgesArray)
            {
                b = new Badge();                                
                if(t["badgeid"] != null) b.BadgeID = (int)t["badgeid"];
                if(t["appid"] != null) b.AppID = (ulong)t["appid"];
                if (t["level"] != null) b.Level = (int)t["level"];
                if (t["completion_time"] != null) b.CompletionTime = UnixTimeStampToDateTime((double)t["completion_time"]);
                if (t["xp"] != null) b.XP = (int)t["xp"];
                if (t["communityitemid"] != null) b.CommunityItemID = (ulong)t["communityitemid"];
                if (t["border_color"] != null) b.BorderColor = (int)t["border_color"];
                if (t["scarcity"] != null) b.Scarcity = (ulong)t["scarcity"];
                list.Add(b);
            }
            return list;
        }

        /// <summary>
        /// Gets level stats for a specified user.
        /// </summary>
        /// <param name="steamID">64-bit Steam ID</param>
        /// <returns>Returns LevelStats</returns>
        public LevelStats GetLevelStats(ulong steamID)
        {            
            string query = @"http://api.steampowered.com/IPlayerService/GetBadges/v1?key=" + APIKey + @"&steamid=" + steamID.ToString();
            string json = Request(query);
            JObject obj = JObject.Parse(json);
            if (obj == null) return null;
            if (obj["response"] == null) return null;
            JToken r = obj["response"];
            LevelStats stats = new LevelStats();
            if (r["player_xp"] != null) stats.PlayerXP = (ulong)r["player_xp"];
            if (r["player_level"] != null) stats.Level = (int)r["player_level"];
            if(r["player_xp_needed_to_level_up"] != null) stats.XPNeeded = (ulong)r["player_xp_needed_to_level_up"];
            if (r["player_xp_needed_current_level"] != null) stats.NeededCurrentLevel = (ulong)r["player_xp_needed_current_level"];
            return stats;            
        }

        /// <summary>
        /// Private helper method for extracting player summaries. 
        /// </summary>
        /// <param name="playerTokens"></param>
        /// <returns></returns>
        private List<PlayerSummary> ExtractPlayerSummaries(IEnumerable<JToken> playerTokens)
        {
            List<PlayerSummary> list = new List<PlayerSummary>();
            PlayerSummary p;
            foreach (JToken player in playerTokens)
            {
                p = new PlayerSummary();
                p.SteamID = (ulong)player["steamid"];
                p.PersonaName = (string)player["personaname"];
                p.ProfileUrl = (string)player["profileurl"];
                p.Avatar = (string)player["avatar"];
                p.AvatarMedium = (string)player["avatarmedium"];
                p.AvatarFull = (string)player["avatarfull"];                
                p.PersonaState = (PlayerSummary.State)((int)player["personastate"]);
                p.CommunityVisibilityState = (PlayerSummary.Visibility)((int)player["communityvisibilitystate"]);                
                if(player["profilestate"] != null)
                    p.ProfileState = ((int)player["profilestate"]) != 0;
                p.LastLogOff = UnixTimeStampToDateTime((double)player["lastlogoff"]);
                if(player["commentpermission"] != null)
                    p.CommentPermission = ((int)player["commentpermission"]) != 0;
                if(p.CommunityVisibilityState == PlayerSummary.Visibility.Public)
                {
                    p.RealName = (string)player["realname"];
                    if (player["primaryclanid"] != null)
                        p.PrimaryClanID = (ulong)player["primaryclanid"];
                    p.TimeCreated = UnixTimeStampToDateTime((double)player["timecreated"]);
                    if (player["gameid"] != null)
                    {
                        p.GameID = (ulong)player["gameid"];
                        p.GameExtraInfo = (string)player["gameextrainfo"];
                        if (player["gameserverip"] != null)
                            p.GameServerIP = (string)player["gameserverip"];
                    }
                    if (player["loccountrycode"] != null)
                        p.LocCountryCode = (string)player["locountrycode"];
                    if (player["locstatecode"] != null)
                        p.LocStateCode = (string)player["locstatecode"];
                    if (player["locityid"] != null)
                        p.LocCityID = (string)player["locityid"];
                }
                list.Add(p);   
            }
            return list;
        }

        /// <summary>
        /// Retrieves information on specified user's steam items such as trading cards, backgrounds, etc. 
        /// </summary>
        /// <param name="steamID">64-bit Steam ID</param>
        /// <returns>Returns nulls if Inventory is private, otherwise returns entire SteamInventory</returns>
        public SteamInventory GetSteamInventory(ulong steamID)
        {
            int off = 0;
            SteamInventory inv = new SteamInventory();
            GetSteamInventory(steamID, off, inv);
            while(inv.Items.Count == off+MaxItemsPerCall)
            {
                off += MaxItemsPerCall;
                GetSteamInventory(steamID, off, inv);
            }
            return inv;
        }

        /// <summary>
        /// Private helper method for retrieven a user's Steam inventory. 
        /// </summary>
        /// <param name="steamID"></param>
        /// <param name="offset"></param>
        /// <param name="inv"></param>
        /// <returns></returns>
        private SteamInventory GetSteamInventory(ulong steamID, int offset, SteamInventory inv)
        {            
            string query = @"http://steamcommunity.com/profiles/" + steamID.ToString() + "/inventory/json/753/6"+"?start="+offset;
            string json = Request(query);
            if (String.IsNullOrEmpty(json)) return null;
            JObject obj = JObject.Parse(json);
            if (obj == null) return null;
            if (bool.Parse(obj["success"].ToString()) != true) return null;
            JToken[] items = obj["rgInventory"].ToArray<JToken>();
            rgInventory i;
            foreach(JToken item in items)
            {
                i = new rgInventory();
                JToken rg = item.Value<JToken>().First();
                i.ID = (ulong)rg["id"];
                i.ClassID = (ulong)rg["classid"];
                i.InstanceID = (ulong)rg["instanceid"];
                i.Amount = (uint)rg["amount"];
                i.Pos = (uint)rg["pos"];
                inv.Items.Add(i);
            }
            JToken[] descriptions = obj["rgDescriptions"].ToArray<JToken>();
            rgDescription d;
            foreach(JToken des in descriptions)
            {
                d = new rgDescription();
                JToken t = des.Value<JToken>().First();
                d.ClassID = (ulong)t["classid"];
                d.InstanceID = (ulong)t["instanceid"];
                string key = d.ClassID.ToString() + "_" + d.InstanceID.ToString();
                if (inv.Descriptions.ContainsKey(key)) continue;
                d.AppID = (ulong)t["appid"];                
                d.IconUrl = (string)t["icon_url"];
                d.IconUrlLarge = (string)t["icon_url_large"];
                d.Name = (string)t["name"];
                d.MarketHashName = (string)t["market_hash_name"];
                d.MarketName = (string)t["market_name"];
                d.NameColor = (string)t["name_color"];
                d.BackgroundColor = (string)t["backgournd_color"];
                d.Type = (string)t["type"];
                d.Tradable = ((int)t["tradable"] != 0);
                d.Marketable = ((int)t["marketable"] != 0);
                d.Commodity = ((int)t["commodity"] != 0);
                d.MarketFeeApp = (ulong)t["market_fee_app"];
                d.MarketTradeableRestriction = (int)t["market_tradable_restriction"];
                d.MakretMarketableRestriction = (int)t["market_marketable_restriction"];
                foreach (JToken val in t["descriptions"].ToArray<JToken>())
                    d.Descriptions.Add((string)val["value"]);
                if(t["actions"] != null)
                    foreach (JToken act in t["actions"].ToArray<JToken>())
                        d.Actions.Add(new rgDescription.Action((string)act["name"], (string)act["link"]));
                foreach (JToken tag in t["tags"].ToArray<JToken>())
                    d.Tags.Add(new rgDescription.Tag((string)tag["internal_name"], (string)tag["name"], (string)tag["category"], (string)tag["category_name"]));
                JToken appData = t["app_data"];
                if(appData != null)
                    d.AppData = new rgDescription.App_Data((ulong)appData["appid"], (int)appData["item_type"]);
                inv.Descriptions.Add(key, d);                
            }
            return inv;
        }        
        
        /// <summary>
        /// Private helper method for making code more readable. 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private JToken[] GetPlayerTokens(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken response = jObject["response"];
            JToken[] players = response["players"].ToArray<JToken>();
            return players;
        }

        /// <summary>
        /// Private helper mehtod which makes the actual API calls. 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static string Request(string query)
        {
            string s = null;
            try
            {
                WebRequest wr = WebRequest.Create(query);
                WebResponse res = wr.GetResponse();
                Stream dataStream = res.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                s = reader.ReadToEnd();
                
            }
            catch(Exception)
            {                                
            }
            return s;        
        }

        /// <summary>
        /// Private helper method which tranlates Unix time to .Net DateTime. 
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
