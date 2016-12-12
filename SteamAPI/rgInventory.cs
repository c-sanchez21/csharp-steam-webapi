using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class rgInventory
    {
        public ulong ID { get; set; }
        public ulong ClassID { get; set;}
        public ulong InstanceID { get; set; }
        public uint Amount { get; set; }
        public uint Pos { get; set; }
        public string DesKey
        {
            get
            {
                return ClassID.ToString() + "_" + InstanceID.ToString();
            }
        }
    }
}
