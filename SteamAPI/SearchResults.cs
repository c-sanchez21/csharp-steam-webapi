using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI
{
    public class SearchResults
    {
        public bool success { get; set; }
        public int start { get; set; }
        public int pagesize { get; set; }
        public int total_count { get; set; }
        public string results_html { get; set; }
    }
}
