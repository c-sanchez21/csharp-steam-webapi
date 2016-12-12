using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class LevelStats
    {
        public ulong PlayerXP { get; set; }
        public int Level { get; set; }
        /// <summary>
        /// XP Needed to Level Up
        /// </summary>
        public ulong XPNeeded { get; set; }        
        /// <summary>
        /// Player XP Needed Current Level
        /// </summary>
        public ulong NeededCurrentLevel { get; set; }
    }
}
