using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using Microsoft.Extensions.Logging;

namespace GoldMember;

public partial class GoldMember
{
    private bool IsClientVip(CCSPlayerController player)
    {
        return _api?.IsPlayerVip(player) ?? false;
    }
    
    private void GiveTempVip(CCSPlayerController player)
    {
        if (_api == null)
            return;
        
        _api.GivePlayerVip(
            player,
            Config.VIPGroup,
            Config.VIPTime != 0 ? Config.VIPTime : GetRoundTime()
        );
    }
    
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;

        if (Config.DebugLogs == true) Logger.LogInformation($"Player '{player.PlayerName}' spawned.");

        if (_api == null) return HookResult.Continue;

        var moneyServices = player.InGameMoneyServices;
        if (moneyServices == null) return HookResult.Continue;

        if (string.IsNullOrWhiteSpace(Config.Money))
            Config.Money = "0";

        var playerPawn = player.Pawn.Value;
        var PlayerPawn = player.PlayerPawn.Value;
        if (playerPawn == null) return HookResult.Continue;
        if (PlayerPawn == null) return HookResult.Continue;

        bool hasPermission = Config.RestrictFlags.Any(restrictionFlags => AdminManager.PlayerHasPermissions(player, restrictionFlags));
        if (hasPermission == true) return HookResult.Continue;

        if (!IsGoldMember(player))
        {
            if (Config.DebugLogs == true) Logger.LogInformation($"Player '{player.PlayerName}' is not GoldMember.");

            return HookResult.Continue;
        }

        Server.NextFrame(() =>
        {
            if (Config.VipCoreEnabled == true)
            {
                if (Config.GiveItems == true && Config.Items != null)
                {
                    if (_api.IsPistolRound() && Config.GiveItemsDuringPistolRound == true && IsClientVip(player))
                    {
                        foreach (string item in Config.Items)
                        {
                            GiveItemTeamAware(player, item);

                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[PistolRound]");
                        }
                    }
                    else if (!_api.IsPistolRound() && IsClientVip(player))
                    {
                        foreach (string item in Config.Items)
                        {
                            GiveItemTeamAware(player, item);

                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[NormalRound]");
                        }
                    }
                }

                if (_api.IsPistolRound() && Config.GiveMoney == true && Config.GiveMoneyInPistolRounds == true)
                {
                    var money = Config.Money;
                    moneyServices.Account = ResolveMoney(moneyServices.Account);

                    if (moneyServices.Account > Config.MaxMoney) moneyServices.Account = Config.MaxMoney;

                    if (Config.DebugLogs == true)
                    {
                        Logger.LogInformation($"Gave {money}$ to {player.PlayerName}.[PistolRound]");
                        Logger.LogInformation($"Player {player.PlayerName} has {moneyServices.Account}$.[PistolRound]");
                    }

                    Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
                }

                if (!_api.IsPistolRound() && Config.GiveMoney == true)
                {
                    var money = Config.Money;
                    moneyServices.Account = ResolveMoney(moneyServices.Account);

                    if (moneyServices.Account > Config.MaxMoney) moneyServices.Account = Config.MaxMoney;

                    if (Config.DebugLogs == true)
                    {
                        Logger.LogInformation($"Gave {money}$ to {player.PlayerName}.[NormalRound]");
                        Logger.LogInformation($"Player {player.PlayerName} has {moneyServices.Account}$.[NormalRound]");
                    }
                }

                if (IsClientVip(player))
                {
                    if (playerPawn != null && PlayerPawn != null)
                    {
                        if (Config.GiveHealth == true)
                        {
                            if (Config.DebugLogs == true) Logger.LogInformation($"Setting health for {player.PlayerName} to {Config.Health}.");

                            playerPawn.Health = Config.Health;
                            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
                        }

                        if (Config.GiveArmor == true)
                        {
                            if (Config.DebugLogs == true) Logger.LogInformation($"Setting armor for {player.PlayerName} to {Config.Armor}.");

                            PlayerPawn.ArmorValue = Config.Armor;
                            Utilities.SetStateChanged(PlayerPawn, "CCSPlayerPawn", "m_ArmorValue");
                        }
                    }
                }

                if (IsClientVip(player) && Config.GiveVIPToPlayer == true)
                {
                    if (Config.DebugLogs == true) Logger.LogInformation($"Gave VIP: {Config.VIPGroup} to player: {player.PlayerName}");
                    
                    GiveTempVip(player);
                }
            }
            else
            {
                if (Config.GiveItems == true && Config.Items != null)
                {
                    if (IsPistolRound() && Config.GiveItemsDuringPistolRound == true)
                    {
                        foreach (string item in Config.Items)
                        {
                            GiveItemTeamAware(player, item);

                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[PistolRound]");
                        }
                    }
                    else if (!IsPistolRound())
                    {
                        foreach (string item in Config.Items)
                        {
                            GiveItemTeamAware(player, item);

                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[NormalRound]");
                        }
                    }
                }

                if (IsPistolRound() && Config.GiveMoney == true && Config.GiveMoneyInPistolRounds == true)
                {
                    var money = Config.Money;
                    moneyServices.Account = ResolveMoney(moneyServices.Account);

                    if (moneyServices.Account > Config.MaxMoney) moneyServices.Account = Config.MaxMoney;

                    if (Config.DebugLogs == true)
                    {
                        Logger.LogInformation($"Gave {money}$ to {player.PlayerName}.[PistolRound]");
                        Logger.LogInformation($"Player {player.PlayerName} has {moneyServices.Account}$.[PistolRound]");
                    }
                }

                if (!IsPistolRound() && Config.GiveMoney == true)
                {
                    var money = Config.Money;
                    moneyServices.Account = ResolveMoney(moneyServices.Account);

                    if (moneyServices.Account > Config.MaxMoney) moneyServices.Account = Config.MaxMoney;

                    if (Config.DebugLogs == true)
                    {
                        Logger.LogInformation($"Gave {money}$ to {player.PlayerName}.[NormalRound]");
                        Logger.LogInformation($"Player {player.PlayerName} has {moneyServices.Account}$.[NormalRound]");
                    }
                }

                if (playerPawn != null && PlayerPawn != null)
                {
                    if (Config.GiveHealth == true)
                    {
                        if (Config.DebugLogs == true) Logger.LogInformation($"Setting health for {player.PlayerName} to {Config.Health}.");

                        playerPawn.Health = Config.Health;
                        Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
                    }

                    if (Config.GiveArmor == true)
                    {
                        if (Config.DebugLogs == true) Logger.LogInformation($"Setting armor for {player.PlayerName} to {Config.Armor}.");

                        PlayerPawn.ArmorValue = Config.Armor;
                        Utilities.SetStateChanged(PlayerPawn, "CCSPlayerPawn", "m_ArmorValue");
                    }
                }
            }

            // Tags (scoreboard clan tag + chat tag) are applied through ChatManager, not written
            // directly here, so they combine cleanly with VIP/admin tags from other providers.
            if (Config.SetTag)
            {
                if (Config.DebugLogs == true) Logger.LogInformation($"Refreshing GoldMember tag for {player.PlayerName}.");
                RefreshTag(player);
            }
        });
        return HookResult.Continue;
    }
}
