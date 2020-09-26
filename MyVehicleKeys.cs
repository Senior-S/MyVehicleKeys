using System.Collections.Generic;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using HarmonyLib;
using System.IO;
using Steamworks;
using Rocket.Unturned.Chat;
using MyVehicleKeys.Model;
using Rocket.API.Collections;

namespace MyVehicleKeys
{
    public class MyVehicleKeys : RocketPlugin<PluginConfiguration>
    {
        public static MyVehicleKeys Instance;
        private sbyte tt = 0;

        protected override void Load()
        {
            var harmony = new Harmony("com.dvt.vehiclekeys");
            harmony.PatchAll();
            Patch.OnPlayerLockVehicle += OnPlayerLockVehicle;
            Provider.onEnemyConnected += OnEnemyConnected;
            if (File.Exists(Utils.path)) return;
            else Utils.CreateInitialFile();
            Logger.Log("[MyVehicleKeys] Plugin loaded correctly!");
            Logger.Log("If you have any error you can contact the owner in discord: Senior S#9583");
            Instance = this;
        }

        private void OnEnemyConnected(SteamPlayer player)
        {
            UnturnedPlayer user = UnturnedPlayer.FromSteamPlayer(player);
            if (Utils.GetPlayerKeys(user.Id) == null)
            {
                PlayerKeys toadd = new PlayerKeys
                {
                    Id = user.Id,
                    Vehicles = new List<uint>(),
                    MaxVehicleKeys = Configuration.Instance.default_max_keys
                };
                Utils.AddPlayer(toadd);
            }
        }

        private void OnPlayerLockVehicle(CSteamID playerId, uint vehicleId, bool locked)
        {
            if (!locked || playerId.ToString().Length < 16)
            {
                return;
            }
            if (tt != 0)
            {
                return;
            }
            tt = 1;
            UnturnedPlayer user = UnturnedPlayer.FromCSteamID(playerId);
            List<uint> keys = Utils.GetPlayerKeys(user.Id);
            int maxkeys = Utils.GetPlayerMaxKeys(user.Id);
            if (keys.Contains(vehicleId))
            {
                tt = 0;
                return;
            }
            if (keys.Count >= maxkeys)
            {
                UnturnedChat.Say(user.CSteamID, Translate("reach_max_keys"), Color.red);
                InteractableVehicle vv = VehicleManager.findVehicleByNetInstanceID(vehicleId);
                VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                            vv.instanceID,
                            user.CSteamID,
                            user.Player.quests.groupID,
                            false
                });
                tt = 0;
                return;
            }
            else
            {
                Utils.AddPlayerKey(vehicleId, user.Id);
                tt = 0;
                UnturnedChat.Say(user.CSteamID, Translate("add_vehicle_key"), Color.green);
            }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"add_vehicle_key", "Now you have this vehicle in your vehicle keys!"},
                    { "add_vehicle_key_fail", "This vehicle is not added to your keys due this vehicle belongs to someone else." },
                    { "reach_max_keys", "You reached the limit of vehicle keys!" },
                    { "any_vehicle_keys", "You don't have any vehicle key!" },
                    { "vehicle_keys", "You have the next vehicle keys:" },
                    { "vehicle_position", "The position of the vehicle {0} has marked in your map." },
                    { "error_arguments_player", "Please specify a player!" },
                    { "error_player", "Player not found!" },
                    { "error_keys", "You don't own this key or this key don't exist." },
                    { "victim_reach_max_keys", "The victim player have the max amount of permitted vehicles!" },
                    { "player_vehicle_transfered", "Vehicle {0} transfered to {1} correctly!" },
                    { "player_vehicle_key_removed", "The vehicle key {0} has removed from your vehicle keys." },
                    { "vehicle_forced_key_removed", "Now this vehicle don't have a key." },
                    { "player_forced_key_removed", "Your vehicle {0} has forced removed from your keys." },
                    { "error_forced_key_removed", "This vehicle don't have a key!" },
                    { "error_vehicle_not_found", "You need to be looking a vehicle!" },
                    { "error_vehicle_not_locked", "You need to see a locked vehicle!" },
                    { "set_max_keys_correct", "Max keys for player {0} setted correctly!" }
                };
            }
        }

        protected override void Unload()
        {
            Patch.OnPlayerLockVehicle -= OnPlayerLockVehicle;
            Logger.Log("[MyVehicleKeys] Plugin loaded correctly!");
            Logger.Log("If you have any error you can contact the owner in discord: Senior S#9583");
        }
    }
}
