using MyVehicleKeys.Helper;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyVehicleKeys.Commands
{
    public class CommandKeys : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "keys";

        public string Help => "Show your actual keys";

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "SS.keys" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            List<uint> keys = Utils.GetPlayerKeys(user.Id);
            if (keys.Count <= 0)
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("any_vehicle_keys"), true);
                return;
            }
            else
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("vehicle_keys"), Color.green, true);
                for (int i = 0; i < keys.Count; i++)
                {
                    InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(keys[i]);
                    if (vehicle == null || vehicle.isExploded || vehicle.isDrowned)
                    {
                        Utils.RemovePlayerKey(keys[i], user.Id);
                    }
                    else
                    {
                        var distance = Vector3.Distance(user.Player.transform.position, vehicle.transform.position);
                        UnturnedChat.Say(user.CSteamID, $"{keys[i]}(Vehicle id: {vehicle.id}, Distance: {Math.Floor(distance)})", Color.white, true);
                    }
                }
            }
        }
    }

    public class CommandFindVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "findvehicle";

        public string Help => "Set the vehicle position in your map";

        public string Syntax => "<vehicle id>";

        public List<string> Aliases => new List<string> { "fv", "findv", "fvehicle" };

        public List<string> Permissions => new List<string> { "SS.findvehicle" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            if (command.Length != 1)
            {
                UnturnedChat.Say(user.CSteamID, "Error! Correct usage: /findvehicle "+Syntax, Color.red, true);
                return;
            }

            uint id = uint.Parse(command[0]);
            List<uint> keys = Utils.GetPlayerKeys(user.Id);
            if (keys.Count <= 0 || keys == null)
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("any_vehicle_keys"), true);
                return;
            }
            else if (keys.Contains(id))
            {
                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(id);
                if (vehicle != null || !vehicle.isExploded || !vehicle.isDead)
                {
                    UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("vehicle_position", vehicle.instanceID), Color.green);
                    user.Player.quests.replicateSetMarker(true, vehicle.transform.position, "Your vehicle!");
                }
                else
                {
                    UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("vehicle_explode"), Color.red);
                    Utils.RemovePlayerKey(id, user.Id);
                }
            }
            else
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_keys"), true);
            }
        }
    }

    public class CommandGiftVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "giftvehicle";

        public string Help => "Gift a specific vehicle to other player.";

        public string Syntax => "<player> <vehicle id>";

        public List<string> Aliases => new List<string> { "gv", "giftv", "gvehicle" };

        public List<string> Permissions => new List<string> { "SS.giftvehicle" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            uint id = uint.Parse(command[1]);
            string vic = command[0];
            Player player = PlayerTool.getPlayer(vic);
            if (player == null)
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_player"), true);
                return;
            }
            CSteamID vSteamid = player.channel.owner.playerID.steamID;
            UnturnedPlayer victim = UnturnedPlayer.FromCSteamID(vSteamid);
            if (victim == null)
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_player"), true);
                return;
            }
            if (victim == user)
            {
                UnturnedChat.Say(user.CSteamID, "You can transfer a vehicle to yourself.", true);
                return;
            }
            List<uint> userKeys = Utils.GetPlayerKeys(user.Id);
            List<uint> victimKeys = Utils.GetPlayerKeys(victim.Id);
            int vMaxKeys = Utils.GetPlayerMaxKeys(victim.Id);
            if (userKeys.Contains(id))
            {
                if (victimKeys.Count >= vMaxKeys)
                {
                    UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("victim_reach_max_keys"), Color.red, true);
                    return;
                }
                else
                {
                    Utils.RemovePlayerKey(id, user.Id);
                    Utils.AddPlayerKey(id, user.Id);

                    UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("player_vehicle_transfered", id, victim.DisplayName), Color.blue, true);
                }
            }
            else
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_keys"), true);
                return;
            }
        }
    }

    public class CommandDeleteVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "deletevehicle";

        public string Help => "Delete a vehicle from your keys.";

        public string Syntax => "<vehicle id>";

        public List<string> Aliases => new List<string> { "dv", "deletev", "dvehicle" };

        public List<string> Permissions => new List<string> { "SS.deletevehicle" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            if (command.Length != 1)
            {
                UnturnedChat.Say(user, "Error! Correct Usage: /deletevehicle "+Syntax, Color.red);
                return;
            }
            uint id = uint.Parse(command[1]);
            string error = Utils.RemovePlayerKey(id, user.Id);
            if (error == "nokey")
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_keys"), true);
                return;
            }
            else
            {
                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(id);
                VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                            vehicle.instanceID,
                            user.CSteamID,
                            user.Player.quests.groupID,
                            false
                });
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("player_vehicle_key_removed", vehicle.instanceID), Color.red, true);
            }
        }
    }

    public class CommandRemoveVehicleKey : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "removevehiclekey";

        public string Help => "Remove a key from a vehicle.";

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string> { "rvk", "removevk", "rvkey" };

        public List<string> Permissions => new List<string> { "SS.removevehiclekey" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            var vehicle = RaycastHelper.GetVehicleFromHits(RaycastHelper.RaycastAll(new Ray(user.Player.look.aim.position, user.Player.look.aim.forward), 4f, RayMasks.VEHICLE));
            if (vehicle != null)
            {
                string Owner = Utils.CheckVehicleOwner(vehicle.instanceID);
                if (Owner == null)
                {
                    UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_forced_key_removed"), true);
                    return;
                }
                else
                {
                    Utils.RemovePlayerKey(vehicle.instanceID, Owner);
                    UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("vehicle_forced_key_removed"), Color.green, true);
                    VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                    {
                            vehicle.instanceID,
                            user.CSteamID,
                            user.Player.quests.groupID,
                            false
                    });
                    CSteamID cs = new CSteamID(ulong.Parse(Owner));
                    UnturnedPlayer victim = UnturnedPlayer.FromCSteamID(cs);
                    if (victim is UnturnedPlayer)
                    {
                        UnturnedChat.Say(victim.CSteamID, MyVehicleKeys.Instance.Translate("player_forced_key_removed"), Color.red, true);
                    }
                }
            }
            else
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_vehicle_not_found"), Color.green, true);
                return;
            }
        }
    }

    public class CommandSetPlayerMaxKeys : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "setplayermaxkeys";

        public string Help => "Set the max keys for a player.";

        public string Syntax => "<player> <keys>";

        public List<string> Aliases => new List<string> { "spmk", "setpmk", "spmaxkeys", "setplayermk" };

        public List<string> Permissions => new List<string> { "SS.setplayermaxkeys" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer user = (UnturnedPlayer)caller;
            if (command.Length != 2)
            {
                UnturnedChat.Say(user, "Error! Correct Usage: /setplayermaxkeys "+Syntax, Color.red);
                return;
            }
            string playerName = command[0];
            int keys = int.Parse(command[1]);
            Player player1 = PlayerTool.getPlayer(playerName);
            if (player1 == null)
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_player"), true);
                return;
            }
            UnturnedPlayer victim = UnturnedPlayer.FromName(playerName);
            if (victim == null)
            {
                UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("error_player"), true);
                return;
            }

            Utils.SetPlayerMaxKeys(victim.Id, keys);
            UnturnedChat.Say(user.CSteamID, MyVehicleKeys.Instance.Translate("set_max_keys_correct", victim.DisplayName), true);
            return;
        }
    }
}
