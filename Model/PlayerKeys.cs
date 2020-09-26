using System.Collections.Generic;

namespace MyVehicleKeys.Model
{
    public class PlayerKeys
    {
        public string Id { get; set; }

        public List<uint> Vehicles { get; set; }

        public int MaxVehicleKeys { get; set; }
    }
}