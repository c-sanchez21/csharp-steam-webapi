using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    //Classes representing the JSON object that is passed. 
    public class SteamInventory
    {
        public Asset[] assets { get; set; }
        public Description[] descriptions { get; set; }
        public bool more_items { get; set; }
        public ulong last_assetid { get; set; }
        public int total_inventory_count { get; set; }
        public bool success { get; set; }
        public int rwgrsn { get; set; }

        public class Asset
        {
            public ulong appid { get; set; }
            public int contextid { get; set; }
            public ulong assetid { get; set; }
            public ulong classid { get; set; }
            public ulong instanceid { get; set; }
            public uint amount { get; set; }
            public string DescriptionKey
            {
                get
                {
                    return classid.ToString() + "_" + instanceid.ToString();
                }
            }
        }

        public class Description
        {
            public ulong appid { get; set; }
            public ulong classid { get; set; }
            public ulong instanceid { get; set; }
            public int currency { get; set; }
            public string background_color { get; set; }
            public string icon_url { get; set; }
            public string icon_url_large { get; set; }
            public Descriptor[] descriptions { get; set; }
            public bool tradable { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string market_name { get; set; }
            public string market_hash_name { get; set; }
            public int commodity { get; set; }
            public int market_tradable_restriction { get; set; }
            public int market_marketable_restriction { get; set; }
            public bool marketable { get; set; }
            public Tag[] tags { get; set; }
            public DateTime item_expiration { get; set; }
            public Action[] actions { get; set; }
            public string[] fraudwarnings { get; set; }
            public string DescriptionKey
            {
                get
                {
                    return classid.ToString() + "_" + instanceid.ToString();
                }
            }
        }

        public class Descriptor
        {
            public string type { get; set; }
            public string value { get; set; }
            public string color { get; set; }
        }

        public class Tag
        {
            public string category { get; set; }
            public string internal_name { get; set; }
            public string localized_category_name { get; set; }
            public string localized_tag_name { get; set; }
            public string color { get; set; }
        }

        public class Action
        {
            public string link { get; set; }
            public string name { get; set; }
        }
    }
}
