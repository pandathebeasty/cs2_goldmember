using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace GoldMember;

public partial class GoldMember
{
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;
        
        AddTimer(1.0f, () =>
        {
            if (player == null || !player.IsValid) return;
            if (!IsGoldMember(player)) return;
            
            if (Config.WelcomeMessage)
            {
                player.PrintToChat(ReplaceColorPlaceholders(string.Format(Localizer["gold.Welcome"], player.PlayerName)));
            }
            
            if (Config.AnnounceJoinLeave)
            {
                Server.PrintToChatAll(ReplaceColorPlaceholders(string.Format(Localizer["gold.JoinAnnounce"], player.PlayerName)));
            }
        });

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;
        
        if (Config.AnnounceJoinLeave && IsGoldMember(player))
        {
            Server.PrintToChatAll(ReplaceColorPlaceholders(string.Format(Localizer["gold.LeaveAnnounce"], player.PlayerName)));
        }

        return HookResult.Continue;
    }
    
    public void OnGoldMemberCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;

        var dnsList = string.Join(", ", Config.NameDns);
        var itemsString = string.Join(", ", Config.Items.Select(item =>
            new CultureInfo("en-US", false).TextInfo.ToTitleCase(item.Replace("weapon_", ""))));

        player.PrintToChat(ReplaceColorPlaceholders(string.Format(Localizer["gold.InfoHeader"], dnsList)));

        // List the concrete perks that are enabled.
        if (Config.GiveItems && !string.IsNullOrWhiteSpace(itemsString))
            player.PrintToChat(ReplaceColorPlaceholders(string.Format(Localizer["gold.InfoItems"], itemsString)));
        if (Config.GiveHealth)
            player.PrintToChat(ReplaceColorPlaceholders(string.Format(Localizer["gold.InfoHealth"], Config.Health)));
        if (Config.GiveArmor)
            player.PrintToChat(ReplaceColorPlaceholders(string.Format(Localizer["gold.InfoArmor"], Config.Armor)));
        if (Config.GiveMoney)
            player.PrintToChat(ReplaceColorPlaceholders(string.Format(Localizer["gold.InfoMoney"], Config.Money)));

        player.PrintToChat(ReplaceColorPlaceholders(IsGoldMember(player)
            ? Localizer["gold.InfoYouAreMember"]
            : string.Format(Localizer["gold.InfoHowTo"], dnsList)));
    }
    
    public void OnGmListCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid) return;

        var members = Utilities.GetPlayers()
            .Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false } && IsGoldMember(p))
            .Select(p => p.PlayerName)
            .ToList();

        if (members.Count == 0)
        {
            player.PrintToChat(ReplaceColorPlaceholders(Localizer["gold.ListEmpty"]));
            return;
        }

        player.PrintToChat(ReplaceColorPlaceholders(string.Format(Localizer["gold.ListHeader"], members.Count)));
        player.PrintToChat(ReplaceColorPlaceholders(string.Join(", ", members)));
    }
    
    public void OnReloadCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null)
        {
            if (!player.IsValid) return;
            if (!AdminManager.PlayerHasPermissions(player, Config.AdminReloadPermission))
            {
                player.PrintToChat(ReplaceColorPlaceholders(Localizer["gold.NoPermission"]));
                return;
            }
        }
        
        var loaded = LoadConfigFromDisk();
        if (loaded != null)
        {
            Config = loaded;
            if (player != null) player.PrintToChat(ReplaceColorPlaceholders(Localizer["gold.Reloaded"]));
            else Console.WriteLine("[GoldMember] Config reloaded.");
        }
    }

    private GoldMemberConfig? LoadConfigFromDisk()
    {
        try
        {
            if (!File.Exists(CfgPath)) return null;
            var json = File.ReadAllText(CfgPath);
            return System.Text.Json.JsonSerializer.Deserialize<GoldMemberConfig>(json);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[GoldMember] Failed to reload config: {ex.Message}");
            return null;
        }
    }
}
