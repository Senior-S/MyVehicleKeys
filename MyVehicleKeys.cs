using System;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using HarmonyLib;
using Steamworks;
using OpenMod.Core.Helpers;
using System.Collections.Generic;
using OpenMod.Unturned.Users;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using SDG.Unturned;
using MyVehicleKeys.Providers;

[assembly: PluginMetadata("SS.MyVehicleKeys", DisplayName = "MyVehicleKeys", Author = "Senior S")]
namespace MyVehicleKeys
{
    public class MyVehicleKeys : OpenModUnturnedPlugin
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<MyVehicleKeys> m_Logger;
        private readonly IUserManager m_UserManager;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        private sbyte tt = 0;

        public MyVehicleKeys(
            IStringLocalizer stringLocalizer,
            ILogger<MyVehicleKeys> logger,
            IUserManager userManager,
            MyVehicleKeysManager myVehicleKeysManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
            m_UserManager = userManager;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        protected override async UniTask OnLoadAsync()
        {
            var harmony = new Harmony("com.dvt.vehiclekeys");
            harmony.PatchAll();
            Patch.OnPlayerLockVehicle += OnPlayerLockVehicle;
            m_Logger.LogInformation("Plugin loaded correctly!");
        }

        #region Events
        private void OnPlayerLockVehicle(CSteamID playerId, uint vehicleId, bool locked)
        {
            if (!locked || Provider.server == playerId)
            {
                return;
            }
            if (tt != 0)
            {
                return;
            }
            AsyncHelper.RunSync(async () =>
            {
                tt = 1;
                UnturnedUser user = (UnturnedUser)await m_UserManager.FindUserAsync(KnownActorTypes.Player, playerId.ToString(), UserSearchMode.FindById);
                List<uint> keys = await m_MyVehicleKeysManager.GetPlayerKeysAsync(user.Id);
                int maxkeys = await m_MyVehicleKeysManager.GetPlayerMaxKeysAsync(user.Id);
                if (keys.Contains(vehicleId))
                {
                    tt = 0;
                    return;
                }
                if (keys.Count >= maxkeys)
                {
                    await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:reach_max_keys"], System.Drawing.Color.Red);
                    InteractableVehicle vv = VehicleManager.findVehicleByNetInstanceID(vehicleId);
                    await UniTask.SwitchToMainThread();
                    VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                    {
                            vv.instanceID,
                            user.SteamId,
                            user.Player.Player.quests.groupID,
                            false
                    });
                    tt = 0;
                }
                else
                {
                    await m_MyVehicleKeysManager.AddPlayerKey(vehicleId, user.Id);
                    tt = 0;
                    await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:add_vehicle_key"], System.Drawing.Color.Green);
                }
            });
        }
        #endregion

        protected override async UniTask OnUnloadAsync()
        {
            // await UniTask.SwitchToMainThread();
            Patch.OnPlayerLockVehicle -= OnPlayerLockVehicle;
            Harmony.UnpatchAll();
            m_Logger.LogInformation("Plugin unloaded correctly!");
        }
    }
}
