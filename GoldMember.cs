using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Reflection;
using System.Globalization;
using Microsoft.Extensions.Logging;
using VipCoreApi;

namespace GoldMember;
public class GoldMemberConfig: BasePluginConfig
{
    [JsonPropertyName("NameDns")]
    public List<string> NameDns { get; set; } = new List<string>();
    [JsonPropertyName("RestrictFlags")]
    public List<string> RestrictFlags { get; set; } = new List<string>();
    [JsonPropertyName("GiveItems")]
    public bool GiveItems { get; set; } = true;
    [JsonPropertyName("GiveItemsDuringPistolRound")]
    public bool GiveItemsDuringPistolRound { get; set; } = false;
    [JsonPropertyName("Items")]
    public List<string> Items { get; set; } = new List<string>();
    [JsonPropertyName("GiveHealth")]
    public bool GiveHealth { get; set; } = true;
    [JsonPropertyName("Health")]
    public int Health { get; set; } = 100;
    [JsonPropertyName("GiveArmor")]
    public bool GiveArmor { get; set; } = true;
    [JsonPropertyName("Armor")]
    public int Armor { get; set; } = 100;
    [JsonPropertyName("GiveMoney")]
    public bool GiveMoney { get; set; } = true;
    [JsonPropertyName("GiveMoneyInPistolRounds")]
    public bool GiveMoneyInPistolRounds { get; set; } = false;
    [JsonPropertyName("Money")]
    public string Money { get; set; } = "1000";
    [JsonPropertyName("VipCoreEnabled")]
    public bool VipCoreEnabled { get; set; } = true;
    [JsonPropertyName("GiveVIPToPlayer")]
    public bool GiveVIPToPlayer { get; set; } = false;
    [JsonPropertyName("VIPGroup")]
    public string VIPGroup { get; set; } = "";
    [JsonPropertyName("VIPTime")]
    public int VIPTime { get; set; } = 2;  
    [JsonPropertyName("SetClanTag")]
    public bool SetClanTag { get; set; } = false;       
    [JsonPropertyName("ClanTag")]
    public string ClanTag { get; set; } = "GoldMember®";
    [JsonPropertyName("ShowAds")]
    public bool ShowAds { get; set; } = true;       
    [JsonPropertyName("AdsTimer")]
    public float AdsTimer { get; set; } = 60.0f;
    [JsonPropertyName("RestrictedCommands")]
    public List<string> RestrictedCommands { get; set; } = new List<string>();
    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 10;
}
    
[MinimumApiVersion(246)]
public class GoldMember : BasePlugin, IPluginConfig<GoldMemberConfig>
{
    public override string ModuleName => "Gold Member";
    public override string ModuleVersion => "0.1.2";
    public override string ModuleAuthor => "panda";
    public override string ModuleDescription => "Benefits for those who have DNS in name (https://github.com/pandathebeasty/cs2_goldmember)";
    public GoldMemberConfig Config { get; set; }  = new GoldMemberConfig();
    private IVipCoreApi? _api;
    private PluginCapability<IVipCoreApi> PluginCapability { get; } = new("vipcore:core");
    private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
    private static readonly string CfgPath = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", AssemblyName, $"{AssemblyName}.json");
    
    private void UpdateConfig<T>(T config) where T : BasePluginConfig, new()
    {
        var newCfgVersion = new T().Version;

        if (config.Version == newCfgVersion)
            return;

        config.Version = newCfgVersion;
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var updatedJsonContent = JsonSerializer.Serialize(config, options);
        File.WriteAllText(CfgPath, updatedJsonContent);

        Console.WriteLine($"Configuration file updated for V{newCfgVersion}.");
    }
    
    private static readonly Dictionary<string, char> ColorMap = new Dictionary<string, char>
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

    private string ReplaceColorPlaceholders(string message)
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

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        if (Config.VipCoreEnabled)
        {
            _api = PluginCapability.Get();
            if (_api == null) return;
            Logger.LogInformation("RUNNING WITH VIP CORE!");
        }
        else
            Logger.LogInformation("RUNNING WITHOUT VIP CORE!");

        foreach (string command in Config.RestrictedCommands)
        {
            AddCommandListener($"css_{command}", (player, args) => RestrictCommands(player, command));
        }
    }

    public void OnConfigParsed(GoldMemberConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        if (config.Health < 100)
            config.Health = 100;

        UpdateConfig(config);    
    }
    
	public bool IsPistolRound()
    {
        var gamerules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules;
        var halftime = ConVar.Find("mp_halftime")!.GetPrimitiveValue<bool>();
        var maxrounds = ConVar.Find("mp_maxrounds")!.GetPrimitiveValue<int>();

        if (gamerules == null) return false;
        return gamerules.TotalRoundsPlayed == 0 || (halftime && maxrounds / 2 == gamerules.TotalRoundsPlayed) || gamerules.GameRestart;
    }

    public int GetRoundTime()
    {
        return ConVar.Find("mp_roundtime")!.GetPrimitiveValue<int>();
    }

    public void PrintAds(CCSPlayerController? player)
    {
        if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV)
            return;

        bool isGoldMember = Config.NameDns.Any(nameDns => player.PlayerName.IndexOf(nameDns, StringComparison.OrdinalIgnoreCase) >= 0);

        string itemsString = string.Join(", ", Config.Items.Select(item => new CultureInfo("en-US", false).TextInfo.ToTitleCase(item.Replace("weapon_", ""))));

        if (float.IsNaN(Config.AdsTimer))
            Config.AdsTimer = 60.0f;

        if (!isGoldMember && Config.ShowAds)
        {
            AddTimer(Config.AdsTimer,() =>
            {
                player.PrintToChat(ReplaceColorPlaceholders(string.IsNullOrWhiteSpace(itemsString)
                    ? string.Format(Localizer["gold.BecomeGoldMemberMsgWithoutItems"], (object)string.Join(", ", Config.NameDns))
                    : string.Format(Localizer["gold.BecomeGoldMemberMsg"], (object)string.Join(", ", Config.NameDns), itemsString)));
            }, TimerFlags.REPEAT);
            return;
        }

        if(Config.ShowAds)
        {
            AddTimer(Config.AdsTimer, () =>
            {
                player.PrintToChat(ReplaceColorPlaceholders(string.IsNullOrWhiteSpace(itemsString)
                    ? Localizer["gold.IsGoldMemberMsgWithoutItems"]
                    : string.Format(Localizer["gold.IsGoldMemberMsg"], (object)itemsString)));
            }, TimerFlags.REPEAT);
        }
    }
    
    public HookResult RestrictCommands(CCSPlayerController? player, string command)
    {
        if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV)
            return HookResult.Continue;

        bool isGoldMember = Config.NameDns.Any(nameDns => player.PlayerName.IndexOf(nameDns, StringComparison.OrdinalIgnoreCase) >= 0);

        if (!isGoldMember)
        {
            player.PrintToChat(ReplaceColorPlaceholders(Localizer["gold.RestrictedCommand"]));
            return HookResult.Continue;
        }
        
        return HookResult.Continue;
    }

	[GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null || player.IsBot || player.IsHLTV) return HookResult.Continue;
        
        PrintAds(player);
        
        return HookResult.Continue;
    }
	
	[GameEventHandler(HookMode.Post)]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV)
            return HookResult.Continue;

        if (_api == null) return HookResult.Continue;    

        var moneyServices = player.InGameMoneyServices;
        if (moneyServices == null) return HookResult.Continue;
        if (string.IsNullOrWhiteSpace(Config.Money)) return HookResult.Continue;

        var playerPawn = player.Pawn.Value;
        var PlayerPawn = player.PlayerPawn.Value;
        if (playerPawn == null) return HookResult.Continue;
        if (PlayerPawn == null) return HookResult.Continue;

        bool hasPermission = Config.RestrictFlags.Any(restrictionFlags => AdminManager.PlayerHasPermissions(player, restrictionFlags));
        bool isGoldMember = Config.NameDns.Any(nameDns => player.PlayerName.IndexOf(nameDns, StringComparison.OrdinalIgnoreCase) >= 0);
        
        if (isGoldMember == true)
        {
            if (hasPermission == false)
            {
                Server.NextFrame(() =>
                {
                    if (Config.VipCoreEnabled)
                    {
                        if (Config.GiveItems && Config.Items != null)
                        {
                            if (_api.IsPistolRound() && Config.GiveItemsDuringPistolRound)
                            {
                                foreach (string item in Config.Items)
                                {
                                    if ((player.TeamNum == 2 && item.Trim() == "weapon_molotov") || (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade"))
                                        player.GiveNamedItem(item.Trim());
                                    else
                                        player.GiveNamedItem(item.Trim());
                                }
                            }
                            else if (!_api.IsPistolRound())
                            {
                                foreach (string item in Config.Items)
                                {
                                    if ((player.TeamNum == 2 && item.Trim() == "weapon_molotov") || (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade"))
                                        player.GiveNamedItem(item.Trim());
                                    else
                                        player.GiveNamedItem(item.Trim());
                                }
                            }
                        }

                        if (_api.IsPistolRound() && Config.GiveMoney && Config.GiveMoneyInPistolRounds)
                        {
                            moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);
                            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
                        }

                        if (!_api.IsPistolRound() && Config.GiveMoney)
                        {
                            moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);
                            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
                        }
                        
                        if (!_api.IsClientVip(player))
                        {   
                            if (playerPawn != null && PlayerPawn != null)
                            {
                                if (Config.GiveHealth)
                                {
                                    playerPawn.Health = Config.Health;
                                    Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
                                }
                                
                                if (Config.GiveArmor)
                                {    
                                    PlayerPawn.ArmorValue = Config.Armor;
                                    Utilities.SetStateChanged(PlayerPawn, "CCSPlayerPawn", "m_ArmorValue");
                                }   PlayerPawn.ArmorValue = Config.Armor;
                            }
                        }

                        if (!_api.IsClientVip(player) && Config.GiveVIPToPlayer)
                            _api.GiveClientTemporaryVip(player, Config.VIPGroup, Config.VIPTime != 0 ? Config.VIPTime : GetRoundTime());
                    }
                    else
                    {
                        if (Config.GiveItems && Config.Items != null)
                        {
                            if (IsPistolRound() && Config.GiveItemsDuringPistolRound)
                            {
                                foreach (string item in Config.Items)
                                {
                                    if ((player.TeamNum == 2 && item.Trim() == "weapon_molotov") || (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade"))
                                        player.GiveNamedItem(item.Trim());
                                    else
                                        player.GiveNamedItem(item.Trim());
                                }
                            }
                            else if (!IsPistolRound())
                            {
                                foreach (string item in Config.Items)
                                {
                                    if ((player.TeamNum == 2 && item.Trim() == "weapon_molotov") || (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade"))
                                        player.GiveNamedItem(item.Trim());
                                    else
                                        player.GiveNamedItem(item.Trim());
                                }
                            }
                        }

                        if (IsPistolRound() && Config.GiveMoney && Config.GiveMoneyInPistolRounds)
                        {
                            moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);
                            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
                        }
                        
                        if (!IsPistolRound() && Config.GiveMoney)
                        {
                            moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);
                            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
                        }
                        
                        if (playerPawn != null && PlayerPawn != null)
                        {
                            if (Config.GiveHealth)
                            {
                                playerPawn.Health = Config.Health;
                                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
                            }

                            if (Config.GiveArmor)
                            {    
                                PlayerPawn.ArmorValue = Config.Armor;
                                Utilities.SetStateChanged(PlayerPawn, "CCSPlayerPawn", "m_ArmorValue");
                            }
                        }
                    }
                    
                    if(Config.SetClanTag && Config.ClanTag != null && isGoldMember)
                    {
                        player.Clan = Config.ClanTag;
                        Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
                    }
                });
            }
        }
        return HookResult.Continue;
    }
}