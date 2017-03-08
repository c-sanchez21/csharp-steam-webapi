using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SteamAPI
{    
    public class API
    {
        #region Fields
        private string APIKey;
        const int MaxItemsPerCall = 5000;//The max number of inventory items that are shown per call
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
            if (String.IsNullOrEmpty(json)) return null;
            JObject jObject = JObject.Parse(json);
            if (jObject == null) return null;
            JToken apps = jObject["applist"]["apps"];
            JToken[] applist = apps["app"].ToArray<JToken>();
            foreach (JToken app in applist)
                list.Add(new App((ulong)app["appid"], (string)app["name"]));
            return list;
        }

        /// <summary>
        /// Returns the app details including price of the specified app id
        /// </summary>
        /// <param name="appid">The app ID</param>
        /// <returns></returns>
        public static AppDetails GetAppDetails(ulong appid)
        {
            string query = @"http://store.steampowered.com/api/appdetails?appids=" + appid;
            string jsonString = Request(query);
            if (String.IsNullOrEmpty(jsonString)) return null;
            JObject jObject = JObject.Parse(jsonString);
            string key = appid.ToString();
            if (jObject == null || jObject[key] == null) return null;
            return jObject[key].ToObject<AppDetails>();
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
            if (String.IsNullOrEmpty(jsonString)) return null;
            JObject jObject = JObject.Parse(jsonString);
            if (jObject == null) return null;
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
            JToken[] tokens = GetPlayerTokens(Request(query));
            if (tokens == null) return null;
            list.AddRange(tokens);
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
                    JToken[] tokens = GetPlayerTokens(Request(sb.ToString()));
                    if (tokens == null) return null;
                    players.AddRange(tokens); //Add the Player JTokens
                    count = 0;
                    sb = new StringBuilder(query);
                }
            }
            if (count > 0)
            {
                sb.Remove(sb.Length - 1, 1);//Remove Last '+'
                JToken[] tokens = GetPlayerTokens(Request(sb.ToString()));
                if (tokens == null) return null;
                players.AddRange(tokens);                
            }
            return ExtractPlayerSummaries(players);            
        }

        /// <summary>
        /// GetOwnedGames returns a list of games that a player owns along with some playtime information, if the profile is publicly visible.
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
        /// Get a summary of badge information for the specified user.
        /// </summary>
        /// <param name="steamID">64-bit Steam ID</param>
        /// <returns>Returns a players badge summary</returns>
        public BadgeSummary GetBadgeSummary(ulong steamID)
        {            
            string query = @"http://api.steampowered.com/IPlayerService/GetBadges/v1?key=" + APIKey + @"&steamid=" + steamID.ToString();
            string json = Request(query);
            if (String.IsNullOrEmpty(json)) return null;
            JObject obj = JObject.Parse(json);
            if (obj == null || obj["response"] == null) return null;
            BadgeSummary bs = obj["response"].ToObject<BadgeSummary>();
            return bs;           
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
                if (player == null) continue;
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
        /// Retrieves the steam inventory of the specified user. 
        /// NOTE 2017-Jan: This call seems to be highly rate limited. (About 3 every 2 minutes) 
        /// </summary>
        /// <param name="steamID">64-bit Steam ID</param>
        /// <param name="appid">The game ID. Steam = 753, Dota 2 = 570 </param>
        /// <param name="contextid"> The context ID</param>
        /// <param name="startAssetID">The asset ID from which to start retrieving items.</param>        
        /// <returns>Returns nulls if Inventory is private, otherwise returns entire the specified inventory.</returns>
        public static SteamInventory GetSteamInventory(ulong steamID, ulong appid, int contextid, ulong startAssetID = 0)
        {
            string query = @"http://steamcommunity.com/inventory/" + steamID.ToString() + "/" + appid.ToString() + "/" + contextid.ToString() + "?l=english&count=" + MaxItemsPerCall;
            if (startAssetID > 0)
                query += "&start_assetid=" + startAssetID.ToString();
            string json = Request(query);
            if (String.IsNullOrEmpty(json)) return null;
            SteamInventory inv = JsonConvert.DeserializeObject<SteamInventory>(json);
            return inv;
        }

        /// <param name="steamID">64-bit Steam ID</param>
        /// <param name="appid">The game ID. Steam = 753, Dota 2 = 570 </param>
        /// <param name="contextid"> The context ID</param>
        /// <param name="startAssetID">The asset ID from which to start retrieving items.</param>        
        /// <returns>Returns nulls if Inventory is private, otherwise returns entire the specified inventory.</returns>
        public SteamInventory GetInventory(ulong steamID, ulong appid, int contextid, ulong startAssetID = 0)
        {
            return API.GetSteamInventory(steamID, appid, contextid, startAssetID);
        }
        
        /// <summary>
        /// Method for retrieving Steam market prices of the specified item.
        /// </summary>
        /// <param name="marketHashName">The market hash name of the item</param>
        /// <param name="appid"> Optional App ID parameter. Default is Steam (753) </param>
        /// <param name="currency">Optional int currency parameter. Default is USD (1) </param>
        /// <returns></returns>
        public static PriceOverview GetPrice(string marketHashName, ulong appid = 753, int currency = 1)
        {
            if (String.IsNullOrEmpty(marketHashName)) return null;
            string query =
                @"http://steamcommunity.com/market/priceoverview/?currency=" + currency + "&appid=" + appid + "&market_hash_name=" + marketHashName;
            string json = Request(query);
            if (String.IsNullOrEmpty(json)) return null;
            PriceOverview overview = JsonConvert.DeserializeObject<PriceOverview>(json);
            return overview;
        }

        /*
        public void GetItemOrderHistory(SellOrder s, SteamWeb sw)
        {
            string submitURL = @"http://steamcommunity.com/market/itemordershistogram/";
            NameValueCollection data = new NameValueCollection();
            data.Add("sessionid", sw.SessionId);
            data.Add("appid", s.Item.AppID.ToString());
            data.Add("contextid", s.Item.ContextID.ToString());
            data.Add("assetid", s.Item.ID.ToString());
            data.Add("amount", "1");
            data.Add("price", s.Price.ToString());
            string referer = @"http://steamcommunity.com/market/"; //<- Wont work without this
            string resp = sw.Fetch(submitURL, "GET", data, false, referer);//Tried true & false but wont work without Referer
            Console.WriteLine(resp);
            OnActionCompleted(s);
        }        
		data: {
			country: g_strCountryCode,
			language: g_strLanguage,
			currency: typeof( g_rgWalletInfo ) != 'undefined' && g_rgWalletInfo['wallet_currency'] != 0 ? g_rgWalletInfo['wallet_currency'] : 1,
			item_nameid: item_nameid,
			two_factor: BIsTwoFactorEnabled() ? 1 : 0
            */

        public static ulong GetItemNameID(string marketHashName)
        {
            string url = @"http://steamcommunity.com/market/listings/753/" + marketHashName;
            string webData = ReadTextFromUrl(url);
            return ParseItemNameID(webData);
        }

        const string RegItemNameID = @"(Market_LoadOrderSpread\(.?)(?<NameID>\d+)(.?\);)";
        private static ulong ParseItemNameID(string webData)
        {
            Match bingo;
            bingo = Regex.Match(webData, RegItemNameID);
            if (!bingo.Success) return 0;
            ulong nameID = Convert.ToUInt64(bingo.Groups["NameID"].ToString());
            return nameID;
        }

        private static string ReadTextFromUrl(string url)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(url))
            using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
            {
                return textReader.ReadToEnd();
            }
        }        

        /// <summary>
        /// Private helper method for making code more readable. 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private JToken[] GetPlayerTokens(string json)
        {
            if (String.IsNullOrEmpty(json)) return null;
            JObject jObject = JObject.Parse(json);
            if (jObject == null) return null;
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
            catch(Exception e)
            {
                Console.WriteLine(e);                                
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
