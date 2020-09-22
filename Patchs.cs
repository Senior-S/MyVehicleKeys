using HarmonyLib;
using SDG.Unturned;
using Steamworks;

namespace MyVehicleKeys
{
    public class Patch
    {
        public delegate void PlayerLockVehicle(CSteamID playerId, uint vehicleId, bool locked);
        public static event PlayerLockVehicle OnPlayerLockVehicle;

        [HarmonyPatch]
        public class TellVehicleLock_Patch
        {
            [HarmonyPatch(typeof(VehicleManager), "tellVehicleLock")]
            [HarmonyPrefix]
            static bool VehicleLock(VehicleManager __instance, CSteamID steamID, uint instanceID, CSteamID owner, CSteamID group, bool locked)
            {
                OnPlayerLockVehicle?.Invoke(owner, instanceID, locked);
                return true;
            }
        }
    }
}
