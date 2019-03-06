using System.Collections.Generic;

namespace Tickets.Mode
{
    public class SubmitMode
    {
        public string Code { get; set; }

        public string BuildingCode { get; set; }

        public string Name { get; set; }

        public string BuildingName { get; set; }

        public string BuildingFloor { get; set; }

        public List<string> RoomList { get; set; }

    }
}