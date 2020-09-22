using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MyVehicleKeys.Helper;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
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
    #region CommandKeys
    [Command("keys")]
    [CommandDescription("Show your actual keys")]
    public class CommandKeys : Command
    {
        private readonly ILogger<CommandKeys> m_Logger;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandKeys(IServiceProvider serviceProvider, ILogger<CommandKeys> logger, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_Logger = logger;
            m_StringLocalizer = stringLocalizer;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            List<uint> keys = Utils.GetPlayerKeys(user.Id);

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
                        Utils.RemovePlayerKey(keys[i], user.Id);
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
        private readonly ILogger<CommandFindVehicle> m_Logger;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandFindVehicle(IServiceProvider serviceProvider, ILogger<CommandFindVehicle> logger, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_Logger = logger;
            m_StringLocalizer = stringLocalizer;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(0);
            List<uint> keys = Utils.GetPlayerKeys(user.Id);
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
                    Utils.RemovePlayerKey(id, user.Id);
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
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IConfiguration m_Configuration;

        public CommandGiftVehicle(IServiceProvider serviceProvider, IUserManager userManager, IStringLocalizer stringLocalizer, IPermissionChecker permissionChecker, IConfiguration configuration) : base(serviceProvider)
        {
            m_UserManager = userManager;
            m_StringLocalizer = stringLocalizer;
            m_PermissionChecker = permissionChecker;
            m_Configuration = configuration;
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
            List<uint> userKeys = Utils.GetPlayerKeys(user.Id);
            List<uint> victimKeys = Utils.GetPlayerKeys(victim.Id);
            int vMaxKeys = Utils.GetPlayerMaxKeys(victim.Id);
            if (userKeys.Contains(id))
            {
                if (victimKeys.Count >= vMaxKeys)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:victim_reach_max_keys"]);
                }
                else
                {
                    Utils.RemovePlayerKey(id, user.Id);
                    Utils.AddPlayerKey(id, user.Id);

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
        private readonly ILogger<CommandDeleteVehicle> m_Logger;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDeleteVehicle(IServiceProvider serviceProvider, ILogger<CommandDeleteVehicle> logger, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_Logger = logger;
            m_StringLocalizer = stringLocalizer;
        }

        protected async override Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(0);
            string error = Utils.RemovePlayerKey(id, user.Id);
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

        public CommandRemoveVehicleKey(IServiceProvider serviceProvider, ILogger<CommandRemoveVehicleKey> logger, IUserManager userManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_Logger = logger;
            m_UserManager = userManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected async override Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;
            var vehicle = RaycastHelper.GetVehicleFromHits(RaycastHelper.RaycastAll(new Ray(user.Player.Player.look.aim.position, user.Player.Player.look.aim.forward), 4f, RayMasks.VEHICLE));
            if (vehicle != null)
            {
                string Owner = Utils.CheckVehicleOwner(vehicle.instanceID);
                if (Owner == null)
                {
                    throw new UserFriendlyException(m_StringLocalizer["plugin_translations:error_forced_key_removed"]);
                }
                else
                {
                    Utils.RemovePlayerKey(vehicle.instanceID, Owner);
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

        public CommandSetPlayerMaxKeys(IServiceProvider serviceProvider, ILogger<CommandSetPlayerMaxKeys> logger, IStringLocalizer stringLocalizer, IUserManager userManager) : base(serviceProvider)
        {
            m_Logger = logger;
            m_StringLocalizer = stringLocalizer;
            m_UserManager = userManager;
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

            Utils.SetPlayerMaxKeys(victim.Id, keys);
            await user.PrintMessageAsync(m_StringLocalizer["plugin_translations:set_max_keys_correct", new { playerName = victim.DisplayName }]);
        }
    }
    #endregion
}
