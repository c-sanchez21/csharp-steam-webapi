using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class SteamInventory
    {
        public SteamInventory()
        {
            Items = new List<rgInventory>();
            Descriptions = new Dictionary<string, rgDescription>();
        }

        public List<rgInventory> Items { get; set; }
        public Dictionary<string, rgDescription> Descriptions { get; set; }
    }
}
