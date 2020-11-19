using Microsoft.Extensions.Configuration;
using MyVehicleKeys.Model;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;
using System.Threading.Tasks;
using MyVehicleKeys.Providers;

namespace MyVehicleKeys
{
    public class OnUserConnected : IEventListener<UnturnedUserConnectedEvent>
    {
        private readonly IConfiguration m_Configuration;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        public OnUserConnected(IConfiguration configuration, MyVehicleKeysManager myVehicleKeysManager)
        {
            m_Configuration = configuration;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        public async Task HandleEventAsync(object sender, UnturnedUserConnectedEvent @event)
        {
            UnturnedUser user = @event.User;
            if (await m_MyVehicleKeysManager.GetPlayerKeysAsync(user.Id) == null)
            {
                PlayerKeys toadd = new PlayerKeys
                {
                    Id = user.Id,
                    Vehicles = new System.Collections.Generic.List<uint>(),
                    MaxVehicleKeys = m_Configuration.GetSection("plugin_configuration:vehicle_keys:default_vehicle_keys").Get<int>()
                };
                await m_MyVehicleKeysManager.AddPlayer(toadd);
            }
        }
    }
}
