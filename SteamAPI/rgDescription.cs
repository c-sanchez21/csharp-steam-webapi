using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class rgDescription
    {
        public ulong AppID { get; set; }
        public ulong ClassID { get; set; }
        public ulong InstanceID { get; set; }
        public string IconUrl { get; set; }
        public string IconUrlLarge { get; set; }
        public string IconDragUrl { get; set; }
        public string Name { get; set; }
        public string MarketHashName { get; set; }
        public string MarketName { get; set; }
        public string NameColor { get; set; }
        public string BackgroundColor { get; set; }
        public string Type { get; set; }
        public bool Tradable { get; set; }
        public bool Marketable { get; set; }
        public bool Commodity { get; set; }
        public ulong MarketFeeApp { get; set; }
        public int MarketTradeableRestriction { get; set; }
        public int MakretMarketableRestriction { get; set; }
        public List<string> Descriptions { get; set; }
        public List<Action> Actions { get; set; }
        public List<Tag> Tags { get; set; }
        public App_Data AppData { get; set; }

        public rgDescription()
        {
            Descriptions = new List<string>();
            Actions = new List<Action>();
            Tags = new List<Tag>();            
        }


        public class Action
        {
            public Action() { }
            public Action(string name, string link)
            {
                this.Name = name;
                this.Link = link;
            }

            public string Name { get; set; }
            public string Link { get; set; }
        }

        public class Tag
        {
            public Tag() {}
            public Tag(string iname, string name, string cat, string catName)
            {
                this.InternalName = iname;
                this.Name = name;
                this.Category = cat;
                this.CategoryName = catName;
            }
            public string InternalName { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string CategoryName { get; set; }
        }

        public class App_Data
        {
            public App_Data() { }
            public App_Data(ulong appid, int itemtype)
            {
                this.AppID = appid;
                this.ItemType = itemtype;
            }
            public ulong AppID { get; set; }
            public int ItemType { get; set; }
        }
    }
}
