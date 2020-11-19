using System;
using System.Collections.Generic;

namespace MyVehicleKeys.Model
{
    [Serializable]
    public class Keys
    {
        public Keys()
        {
            PKeys = new List<PlayerKeys>();
        }
        
        public List<PlayerKeys> PKeys { get; set; }
    }
}
