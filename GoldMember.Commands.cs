using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace GoldMember;

public partial class GoldMember
{
    public HookResult RestrictCommands(CCSPlayerController? player, string command)
    {
        if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;

        if (!IsGoldMember(player))
        {
            if (Config.DebugLogs == true) Logger.LogInformation($"Restrict {command} for player: '{player.PlayerName}'.");

            player.PrintToChat(ReplaceColorPlaceholders(Localizer["gold.RestrictedCommand"]));
            return HookResult.Continue;
        }
        return HookResult.Continue;
    }
}
