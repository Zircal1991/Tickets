using System;
using System.Collections.Generic;
using System.Text;

namespace Tickets
{
    public static class Extension
    {

        public static long CurrentTimeMillis(this DateTime dateTime)
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970,1,1,0,0,0,0);
            long currentMillis = (currentTicks - dtFrom.Ticks) / 10000;
            return currentMillis;
        }


    }
}
