using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MyVehicleKeys.Helper;
using MyVehicleKeys.Providers;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace MyVehicleKeys
{
    //#region CommandKeysUI
    //[Command("keysui")]
    //[CommandDescription("Show your actual keys with a UI")]
    //public class CommandKeysUI : Command
    //{
    //    private readonly ILogger<CommandKeysUI> m_Logger;
    //    private readonly IStringLocalizer m_StringLocalizer;

    //    public CommandKeysUI(IServiceProvider serviceProvider, ILogger<CommandKeysUI> logger, IStringLocalizer stringLocalizer) : base(serviceProvider)
    //    {
    //        m_Logger = logger;
    //        m_StringLocalizer = stringLocalizer;
    //    }

    //    protected async override Task OnExecuteAsync()
    //    {
    //        UnturnedUser user = (UnturnedUser)Context.Actor;
    //        List<uint> keys = Utils.GetPlayerKeys(user.Id);

    //        if (keys.Count <= 0)
    //        {
    //            throw new UserFriendlyException(m_StringLocalizer["plugin_translations:any_vehicle_keys"]);
    //        }
    //        else
    //        {
    //            for (int i = 0; i < keys.Count; i++)
    //            {
    //                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(keys[i]);
    //                if (vehicle == null || vehicle.isExploded)
    //                {
    //                    Utils.RemovePlayerKey(keys[i], user.Id);
    //                }
    //                else
    //                {
    //                    string s = keys[i] + " - Name: " + vehicle.transform.name;
    //                    if (Utils.EffectIds.TryGetValue(user.Player.Player, out List<string> value))
    //                    {
    //                        value.Add(s);
    //                        Utils.EffectIds.Remove(user.Player.Player);
    //                        Utils.EffectIds.Add(user.Player.Player, value);
    //                    }
    //                    else
    //                    {
    //                        List<string> list = new List<string>
    //                        {
    //                            s
    //                        };
    //                        Utils.EffectIds.Add(user.Player.Player, list);
    //                    }
    //                }
    //            }
    //            Utils.EffectIds.TryGetValue(user.Player.Player, out List<string> v);
    //            await UniTask.SwitchToMainThread();
    //            await Utils.SendKeysUI(user.Player.Player, v);
    //            m_Logger.LogInformation("Ready");
    //        }
    //    }
    //}
    //#endregion

    #region CommandKeys
    [Command("keys")]
    [CommandDescription("Show your actual keys")]
    public class CommandKeys : Command
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        public CommandKeys(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, MyVehicleKeysManager myVehicleKeysManager) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            List<uint> keys = await m_MyVehicleKeysManager.GetPlayerKeysAsync(user.Id);

            if (keys.Count <= 0)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:any_vehicle_keys"]);
            }
            else
            {
                await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:vehicle_keys"], System.Drawing.Color.Green);
                for (int i = 0; i < keys.Count; i++)
                {
                    InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(keys[i]);
                    if (vehicle == null || vehicle.isExploded)
                    {
                        await m_MyVehicleKeysManager.RemovePlayerKey(keys[i], user.Id);
                    }
                    else
                    {
                        var distance = UnityEngine.Vector3.Distance(user.Player.Player.transform.position, vehicle.transform.position);
                        await user.PrintMessageAsync($"{keys[i]}(Vehicle id: {vehicle.id}, Distance: {Math.Floor(distance)})");
                    }
                }
            }
        }
    }
    #endregion

    #region CommandFindVehicle
    [Command("findvehicle")]
    [CommandAlias("fv")]
    [CommandAlias("findv")]
    [CommandAlias("fvehicle")]
    [CommandSyntax("<vehicle id>")]
    [CommandDescription("Set the vehicle position in your map")]
    public class CommandFindVehicle : Command
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        public CommandFindVehicle(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, MyVehicleKeysManager myVehicleKeysManager) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(0);
            List<uint> keys = await m_MyVehicleKeysManager.GetPlayerKeysAsync(user.Id);
            if (keys.Count <= 0 || keys == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:any_vehicle_keys"]);
            }
            else if (keys.Contains(id))
            {
                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(id);
                if (vehicle != null || !vehicle.isExploded || !vehicle.isDead)
                {
                    await UniTask.SwitchToMainThread();
                    await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:vehicle_position", new { vehicleId = vehicle.instanceID }], System.Drawing.Color.Green);
                    user.Player.Player.quests.replicateSetMarker(true, vehicle.transform.position, "Your vehicle!");
                }
                else
                {
                    await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:vehicle_explode"], System.Drawing.Color.Red);
                    await m_MyVehicleKeysManager.RemovePlayerKey(id, user.Id);
                }
            }
            else
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_keys"]);
            }
        }
    }
    #endregion

    #region CommandGiftVehicle
    [Command("giftvehicle")]
    [CommandAlias("gv")]
    [CommandAlias("giftv")]
    [CommandAlias("gvehicle")]
    [CommandSyntax("<player> <vehicle instance id>")]
    [CommandDescription("Gift a specific vehicle to other player.")]
    public class CommandGiftVehicle : Command
    {
        private readonly IUserManager m_UserManager;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        public CommandGiftVehicle(IServiceProvider serviceProvider, MyVehicleKeysManager myVehicleKeysManager, IUserManager userManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_UserManager = userManager;
            m_StringLocalizer = stringLocalizer;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(1);
            string vic = await Context.Parameters.GetAsync<string>(0);
            Player player = PlayerTool.getPlayer(vic);
            if (player == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_player"]);
            }
            UnturnedUser victim = (UnturnedUser)await m_UserManager.FindUserAsync(KnownActorTypes.Player, player.channel.owner.playerID.steamID.ToString(), UserSearchMode.FindById);
            if (victim == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_player"]);
            }
            if (victim == user)
            {
                throw new UserFriendlyException("You can transfer a vehicle to yourself.");
            }
            List<uint> userKeys = await m_MyVehicleKeysManager.GetPlayerKeysAsync(user.Id);
            List<uint> victimKeys = await m_MyVehicleKeysManager.GetPlayerKeysAsync(victim.Id);
            int vMaxKeys = await m_MyVehicleKeysManager.GetPlayerMaxKeysAsync(victim.Id);
            if (userKeys.Contains(id))
            {
                if (victimKeys.Count >= vMaxKeys)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:victim_reach_max_keys"]);
                }
                else
                {
                    await m_MyVehicleKeysManager.RemovePlayerKey(id, user.Id);
                    await m_MyVehicleKeysManager.AddPlayerKey(id, user.Id);

                    await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:player_vehicle_transfered", new { vehicleId = id, victimName = victim.DisplayName }], System.Drawing.Color.Blue);
                }
            }
            else
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_keys"]);
            }
        }
    }
    #endregion

    #region CommandDeleteVehicle
    [Command("deletevehicle")]
    [CommandAlias("dv")]
    [CommandAlias("deletev")]
    [CommandAlias("dvehicle")]
    [CommandSyntax("<vehicle id>")]
    [CommandDescription("Delete a vehicle from your keys.")]
    public class CommandDeleteVehicle : Command
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        public CommandDeleteVehicle(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, MyVehicleKeysManager myVehicleKeysManager) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(0);
            string error = await m_MyVehicleKeysManager.RemovePlayerKey(id, user.Id);
            if (error == "nokey")
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_keys"]);
            }
            else
            {
                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(id);
                await UniTask.SwitchToMainThread();
                VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                            vehicle.instanceID,
                            user.SteamId,
                            user.Player.Player.quests.groupID,
                            false
                });
                await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:player_vehicle_key_removed", new { vehicleId = vehicle.instanceID }], System.Drawing.Color.Red);
            }
        }
    }
    #endregion

    #region CommandRemoveVehicleKey
    [Command("removevehiclekey")]
    [CommandAlias("rvk")]
    [CommandAlias("removevk")]
    [CommandAlias("rvkey")]
    [CommandDescription("Remove a key from a vehicle!")]
    public class CommandRemoveVehicleKey : Command
    {
        private readonly ILogger<CommandRemoveVehicleKey> m_Logger;
        private readonly IUserManager m_UserManager;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        public CommandRemoveVehicleKey(IServiceProvider serviceProvider, MyVehicleKeysManager myVehicleKeysManager,ILogger<CommandRemoveVehicleKey> logger, IUserManager userManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_Logger = logger;
            m_UserManager = userManager;
            m_StringLocalizer = stringLocalizer;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        protected async override Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;
            var vehicle = RaycastHelper.GetVehicleFromHits(RaycastHelper.RaycastAll(new Ray(user.Player.Player.look.aim.position, user.Player.Player.look.aim.forward), 4f, RayMasks.VEHICLE));
            if (vehicle != null)
            {
                string Owner = await m_MyVehicleKeysManager.CheckVehicleOwner(vehicle.instanceID);
                if (Owner == null)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_forced_key_removed"]);
                }
                else
                {
                    await m_MyVehicleKeysManager.RemovePlayerKey(vehicle.instanceID, Owner);
                    await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:vehicle_forced_key_removed"], System.Drawing.Color.Green);
                    await UniTask.SwitchToMainThread();
                    VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                    {
                            vehicle.instanceID,
                            user.SteamId,
                            user.Player.Player.quests.groupID,
                            false
                    });
                    UnturnedUser victim = (UnturnedUser)await m_UserManager.FindUserAsync(KnownActorTypes.Player, Owner, UserSearchMode.FindById);
                    if (victim is UnturnedUser)
                    {
                        await victim.PrintMessageAsync(m_StringLocalizer["plugin_translations:player_forced_key_removed", new { vehicleId = vehicle.instanceID }], System.Drawing.Color.Red);
                    }
                }
            }
            else
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_vehicle_not_found"]);
            }
        }
    }
    #endregion

    #region CommandSetPlayerMaxKeys
    [Command("setplayermaxkeys")]
    [CommandAlias("spmk")]
    [CommandAlias("setpmk")]
    [CommandAlias("spmaxkeys")]
    [CommandAlias("setplayermk")]
    [CommandSyntax("<player> <keys>")]
    [CommandDescription("Set the max keys for a player.")]
    public class CommandSetPlayerMaxKeys : Command
    {
        private readonly ILogger<CommandSetPlayerMaxKeys> m_Logger;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserManager m_UserManager;
        private readonly MyVehicleKeysManager m_MyVehicleKeysManager;

        public CommandSetPlayerMaxKeys(IServiceProvider serviceProvider, MyVehicleKeysManager myVehicleKeysManager,ILogger<CommandSetPlayerMaxKeys> logger, IStringLocalizer stringLocalizer, IUserManager userManager) : base(serviceProvider)
        {
            m_Logger = logger;
            m_StringLocalizer = stringLocalizer;
            m_UserManager = userManager;
            m_MyVehicleKeysManager = myVehicleKeysManager;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            string playerName = await Context.Parameters.GetAsync<string>(0);
            int keys = await Context.Parameters.GetAsync<int>(1);
            Player player1 = PlayerTool.getPlayer(playerName);
            if (player1 == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_player"]);
            }
            UnturnedUser victim = (UnturnedUser)await m_UserManager.FindUserAsync(KnownActorTypes.Player, playerName, UserSearchMode.FindByNameOrId);
            if (victim == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_player"]);
            }

            await m_MyVehicleKeysManager.SetPlayerMaxKeysAsync(victim.Id, keys);
            await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:set_max_keys_correct", new { playerName = victim.DisplayName }]);
        }
    }
    #endregion
}
