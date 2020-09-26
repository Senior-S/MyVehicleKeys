using Rocket.API;

namespace MyVehicleKeys
{
    public class PluginConfiguration : IRocketPluginConfiguration
    {
        public int default_max_keys;
        public void LoadDefaults()
        {
            default_max_keys = 3;
        }
    }
}
