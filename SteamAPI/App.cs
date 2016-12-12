using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    [Serializable]
    public class App
    {
        public App() { }
        public App(ulong appid, string name)
        {
            this.AppID = appid;
            this.Name = name;
        }

        public ulong AppID { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return this.Name + " : " + this.AppID.ToString();
        }
    }
}
