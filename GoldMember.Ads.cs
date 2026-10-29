using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace GoldMember;

public partial class GoldMember
{
    public void PrintAds()
    {
        string itemsString = string.Join(", ", Config.Items.Select(item => new CultureInfo("en-US", false).TextInfo.ToTitleCase(item.Replace("weapon_", ""))));

        if (float.IsNaN(Config.AdsTimer))
        {
            if (Config.DebugLogs == true) Logger.LogInformation($"AdsTimer setting to 60 seconds by default.");

            Config.AdsTimer = 60.0f;
        }

        var players = Utilities.GetPlayers().Where(x => x is { IsBot: false, Connected: PlayerConnectedState.Connected });

        foreach (var player in players)
        {
            if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV) continue;

            if (!IsGoldMember(player) && Config.ShowAds)
            {
                if (Config.DebugLogs == true) Logger.LogInformation($"Displaying ad to non-GoldMember '{player.PlayerName}'.");

                player.PrintToChat(ReplaceColorPlaceholders(string.IsNullOrWhiteSpace(itemsString)
                    ? string.Format(Localizer["gold.BecomeGoldMemberMsgWithoutItems"], (object)string.Join(", ", Config.NameDns))
                    : string.Format(Localizer["gold.BecomeGoldMemberMsg"], (object)string.Join(", ", Config.NameDns), itemsString)));
                continue;
            }

            if (Config.ShowAds)
            {
                if (Config.DebugLogs == true) Logger.LogInformation($"Displaying ad to GoldMember '{player.PlayerName}'.");

                player.PrintToChat(ReplaceColorPlaceholders(string.IsNullOrWhiteSpace(itemsString)
                    ? Localizer["gold.IsGoldMemberMsgWithoutItems"]
                    : string.Format(Localizer["gold.IsGoldMemberMsg"], (object)itemsString)));
            }
        }
    }
}
