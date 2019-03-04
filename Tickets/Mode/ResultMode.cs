using System;
using System.Collections.Generic;
using System.Text;

namespace Tickets.Mode
{
    class ResultMode
    {
        public string Status { get; set; }

        public List<Dictionary<string,List<Room>>> List { get; set; }

        public string Version { get; set; }

    }
}
