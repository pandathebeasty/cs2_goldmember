using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace GoldMember;

public partial class GoldMember
{
    public static readonly Dictionary<string, char> ColorMap = new Dictionary<string, char>
    {
        { "{default}", ChatColors.Default },
        { "{white}", ChatColors.White },
        { "{darkred}", ChatColors.DarkRed },
        { "{green}", ChatColors.Green },
        { "{lightyellow}", ChatColors.LightYellow },
        { "{lightblue}", ChatColors.LightBlue },
        { "{olive}", ChatColors.Olive },
        { "{lime}", ChatColors.Lime },
        { "{red}", ChatColors.Red },
        { "{lightpurple}", ChatColors.LightPurple },
        { "{purple}", ChatColors.Purple },
        { "{grey}", ChatColors.Grey },
        { "{yellow}", ChatColors.Yellow },
        { "{gold}", ChatColors.Gold },
        { "{silver}", ChatColors.Silver },
        { "{blue}", ChatColors.Blue },
        { "{darkblue}", ChatColors.DarkBlue },
        { "{bluegrey}", ChatColors.BlueGrey },
        { "{magenta}", ChatColors.Magenta },
        { "{lightred}", ChatColors.LightRed },
        { "{orange}", ChatColors.Orange }
    };

    public string ReplaceColorPlaceholders(string message)
    {
        if (!string.IsNullOrEmpty(message) && message[0] != ' ')
        {
            message = " " + message;
        }
        foreach (var colorPlaceholder in ColorMap)
        {
            message = message.Replace(colorPlaceholder.Key, colorPlaceholder.Value.ToString());
        }
        return message;
    }

    public bool IsPistolRound()
    {
        var proxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault();
        var gamerules = proxy?.GameRules;
        if (gamerules == null) return false;

        var halftime = ConVar.Find("mp_halftime")?.GetPrimitiveValue<bool>() ?? false;
        var maxrounds = ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>() ?? 0;

        return gamerules.TotalRoundsPlayed == 0 || (halftime && maxrounds / 2 == gamerules.TotalRoundsPlayed) || gamerules.GameRestart;
    }

    public int GetRoundTime()
    {
        return ConVar.Find("mp_roundtime")?.GetPrimitiveValue<int>() ?? 2;
    }
    
    private static void GiveItemTeamAware(CCSPlayerController player, string rawItem)
    {
        var item = rawItem.Trim();
        
        if (item is "weapon_molotov" or "weapon_incgrenade")
        {
            item = player.TeamNum == 2 ? "weapon_molotov" : "weapon_incgrenade";
        }

        player.GiveNamedItem(item);
    }
    
    public int ResolveMoney(int currentAccount)
    {
        var money = Config.Money?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(money))
            return currentAccount;

        if (money.Contains("++"))
        {
            var parts = money.Split("++", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1 && int.TryParse(parts[^1], out var add))
                return currentAccount + add;

            return currentAccount;
        }

        return int.TryParse(money, out var set) ? set : currentAccount;
    }

    public bool IsGoldMember(CCSPlayerController? player)
    {
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return false;

        if (Config.NameDns == null || Config.NameDns.Count == 0) return false;

        foreach (var nameDns in Config.NameDns)
        {
            if (player.PlayerName.IndexOf(nameDns, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (Config.DebugLogs == true) Logger.LogInformation($"Player '{player.PlayerName}' matches DNS '{nameDns}' and is GoldMember.");
                return true;
            }
        }

        if (Config.DebugLogs == true) Logger.LogInformation($"Player '{player.PlayerName}' does not match any DNS and is not GoldMember.");

        return false;
    }
}
