using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    /// <summary>
    /// This class represents the JSON data from the AppDetails API call
    /// </summary>
    public class AppDetails
    {
        public bool success { get; set; }
        public Data data { get; set; }

        public class Data
        {
            public string type { get; set; }
            public string name { get; set; }
            public int steam_appid { get; set; }
            public int required_age { get; set; }
            public bool is_free { get; set; }
            public string detailed_description { get; set; }
            public string about_the_game { get; set; }
            public string short_description { get; set; }
            public string supported_languages { get; set; }
            public string header_image { get; set; }
            public string website { get; set; }
            public Requirements pc_requirements { get; set; }
            //public string[] mac_requirements { get; set; } //Omitted due to inconsistencies
            //public string[] linux_requirements { get; set; } //Omitted due to inconsistencies
            public string[] developers { get; set; }
            public string[] publishers { get; set; }
            public Price_Overview price_overview { get; set; }
            public int[] packages { get; set; }
            public Package_Groups[] package_groups { get; set; }
            public Platforms platforms { get; set; }
            public Category[] categories { get; set; }
            public Genre[] genres { get; set; }
            public Screenshot[] screenshots { get; set; }
            public Movie[] movies { get; set; }
            public Recommendations recommendations { get; set; }
            public Achievements achievements { get; set; }
            public Release_Date release_date { get; set; }
            public Support_Info support_info { get; set; }
            public string background { get; set; }
        }

        public class Requirements
        {
            public string minimum { get; set; }
            public string recommended { get; set; }
        }

        public class Price_Overview
        {
            public string currency { get; set; }
            public int initial { get; set; }
            public int final { get; set; }
            public int discount_percent { get; set; }
        }

        public class Platforms
        {
            public bool windows { get; set; }
            public bool mac { get; set; }
            public bool linux { get; set; }
        }

        public class Recommendations
        {
            public int total { get; set; }
        }

        public class Achievements
        {
            public int total { get; set; }
            public Highlighted[] highlighted { get; set; }
        }

        public class Highlighted
        {
            public string name { get; set; }
            public string path { get; set; }
        }

        public class Release_Date
        {
            public bool coming_soon { get; set; }
            public string date { get; set; }
        }

        public class Support_Info
        {
            public string url { get; set; }
            public string email { get; set; }
        }

        public class Package_Groups
        {
            public string name { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string selection_text { get; set; }
            public string save_text { get; set; }
            public int display_type { get; set; }
            public string is_recurring_subscription { get; set; }
            public Sub[] subs { get; set; }
        }

        public class Sub
        {
            public int packageid { get; set; }
            public string percent_savings_text { get; set; }
            public int percent_savings { get; set; }
            public string option_text { get; set; }
            public string option_description { get; set; }
            public string can_get_free_license { get; set; }
            public bool is_free_license { get; set; }
            public int price_in_cents_with_discount { get; set; }
        }

        public class Category
        {
            public int id { get; set; }
            public string description { get; set; }
        }

        public class Genre
        {
            public string id { get; set; }
            public string description { get; set; }
        }

        public class Screenshot
        {
            public int id { get; set; }
            public string path_thumbnail { get; set; }
            public string path_full { get; set; }
        }

        public class Movie
        {
            public int id { get; set; }
            public string name { get; set; }
            public string thumbnail { get; set; }
            public Webm webm { get; set; }
            public bool highlight { get; set; }
        }

        public class Webm
        {
            public string _480 { get; set; }
            public string max { get; set; }
        }
    }
}
