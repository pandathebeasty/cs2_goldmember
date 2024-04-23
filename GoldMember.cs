using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Modules.Cvars;
using System;

namespace GoldMember;
public class GoldMemberConfig: BasePluginConfig
{
    [JsonPropertyName("NameDns")]
    public List<string> NameDns { get; set; } = new List<string>();
    [JsonPropertyName("GiveItemsDuringPistolRound")]
    public bool GiveItemsDuringPistolRound { get; set; } = false;
    [JsonPropertyName("Items")]
    public List<string> Items { get; set; } = new List<string>();
    [JsonPropertyName("Health")]
    public int Health { get; set; } = 100;
    [JsonPropertyName("Armor")]
    public int Armor { get; set; } = 100;
    [JsonPropertyName("Money")]
    public string Money { get; set; } = "1000";    
    [JsonPropertyName("ClanTag")]
    public string ClanTag { get; set; } = "GoldMember®";
    [JsonPropertyName("BecomeGoldMemberMsg")]
    public string BecomeGoldMemberMsg { get; set; } = "\u0007[GoldMember] \u0001To become\u0010 GoldMember \u0001you need to have\u0004 {0} \u0001in your name to receive following benefits: \u0010{1}\u0001.";
    [JsonPropertyName("IsGoldMemberMsg")]
    public string IsGoldMemberMsg { get; set; } = "\u0007[GoldMember] \u0001You are a \u0010GoldMember.\u0001 You are receiving: \u0010{1}\u0001.";
}

[MinimumApiVersion(213)]
public class GoldMember : BasePlugin, IPluginConfig<GoldMemberConfig>
{
    public override string ModuleName => "Gold Member";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "Created by fernoski#0001 & modified by panda.";
    public GoldMemberConfig? Config { get; set; }

    public void OnConfigParsed(GoldMemberConfig config)
    {
        Config = config;

        if (config.Health < 100)
            config.Health = 100;
    }

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
    }

    public bool IsPistolRound()
    {
        var gamerules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        var halftime = ConVar.Find("mp_halftime")!.GetPrimitiveValue<bool>();
        var maxrounds = ConVar.Find("mp_maxrounds")!.GetPrimitiveValue<int>();

        if (gamerules == null) return false;
        return gamerules.TotalRoundsPlayed == 0 || (halftime && maxrounds / 2 == gamerules.TotalRoundsPlayed) || gamerules.GameRestart;
    }

    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV)
            return HookResult.Continue;

        bool isGoldMember = false;

        foreach (string nameDns in this.Config.NameDns)
        {
            if (player.PlayerName.IndexOf(nameDns, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isGoldMember = true;
                break;
            }
        }

        string itemsString = string.Join(", ", Config.Items);

        if (!isGoldMember)
        {
            player.PrintToChat(string.Format(Config.BecomeGoldMemberMsg, (object)string.Join(", ", Config.NameDns), (object)itemsString));
            return HookResult.Continue;
        }

        player.PrintToChat(string.Format(Config.IsGoldMemberMsg, (object)itemsString));

        var moneyServices = player.InGameMoneyServices;
        if (moneyServices == null) return HookResult.Continue;
        if (string.IsNullOrWhiteSpace(Config.Money)) return HookResult.Continue;

        if (IsPistolRound() && Config.GiveItemsDuringPistolRound == true)
        {
            foreach (string item in Config.Items)
            {
                if (player.TeamNum == 2 && item.Trim() == "weapon_molotov")
                {
                    player.GiveNamedItem(item.Trim());
                }
                else if (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade")
                {
                    player.GiveNamedItem(item.Trim());
                }
                else
                {
                    player.GiveNamedItem(item.Trim());
                }
            }
        }
        else if (!IsPistolRound())
        {
            foreach (string item in Config.Items)
            {
                if (player.TeamNum == 2 && item.Trim() == "weapon_molotov")
                {
                    player.GiveNamedItem(item.Trim());
                }
                else if (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade")
                {
                    player.GiveNamedItem(item.Trim());
                }
                else
                {
                    player.GiveNamedItem(item.Trim());
                }
            }

            if (Config.Money.Contains("++"))
                moneyServices.Account += int.Parse(Config.Money.Split("++")[1]);
            else
                moneyServices.Account = int.Parse(Config.Money);
        }

        player.Pawn.Value.Health = Config.Health;
        player.PlayerPawn.Value.ArmorValue = Config.Armor;
        player.Clan = Config.ClanTag;

        return HookResult.Continue;
    }
}
