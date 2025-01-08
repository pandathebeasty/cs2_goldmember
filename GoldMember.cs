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
    [JsonPropertyName("MaxMoney")]
    public int MaxMoney { get; set; } = 16000;
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
    [JsonPropertyName("DebugLogs")]
    public bool DebugLogs { get; set; } = false;
    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 12;
}
    
[MinimumApiVersion(296)]
public class GoldMember : BasePlugin, IPluginConfig<GoldMemberConfig>
{
    public override string ModuleName => "Gold Member";
    public override string ModuleVersion => "0.1.6";
    public override string ModuleAuthor => "panda.";
    public override string ModuleDescription => "Benefits for those who have DNS in name (https://github.com/pandathebeasty/cs2_goldmember)";
    public GoldMemberConfig Config { get; set; }  = new GoldMemberConfig();
    public IVipCoreApi? _api;
    public PluginCapability<IVipCoreApi> PluginCapability { get; } = new("vipcore:core");
    public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
    public static readonly string CfgPath = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", AssemblyName, $"{AssemblyName}.json");
    
    public void UpdateConfig<T>(T config) where T : BasePluginConfig, new()
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

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

        if (Config.DebugLogs == true)
        {
            Logger.LogInformation("\u001b[1;32mEvent \u001b[1;33m~OnPlayerConnect~ \u001b[1;32mregistered!");
            Logger.LogInformation("\u001b[1;32mEvent \u001b[1;33m~OnPlayerSpawn~ \u001b[1;32mregistered!");
        }

        if (Config.VipCoreEnabled)
        {
            _api = PluginCapability.Get();
            if (_api == null) return;
            
            if (Config.DebugLogs == true) Logger.LogInformation("\u001b[1;32mRunning with \u001b[1;33mVIPCore\u001b[1;32m!");
        }
        else
        {
            if (Config.DebugLogs == true) Logger.LogInformation("\u001b[1;33mRunning without \u001b[1;31mVIPCore\u001b[1;33m!");
        }    

        foreach (string command in Config.RestrictedCommands)
        {
            AddCommandListener($"{command}", (player, args) => RestrictCommands(player, command));
        }
    }

    public void OnConfigParsed(GoldMemberConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        UpdateConfig(config);  
        
        if (Config.DebugLogs == true)
        {
            string configJson = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            Logger.LogInformation("Loaded Config:\n{ConfigJson}", configJson);
        }

        if (config.Health < 100)
        {
            if (Config.DebugLogs == true) Logger.LogInformation("Health value less than 100, setting it back to default.");
            config.Health = 100;
        }  
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

    public void PrintAds()
    {
        string itemsString = string.Join(", ", Config.Items.Select(item => new CultureInfo("en-US", false).TextInfo.ToTitleCase(item.Replace("weapon_", ""))));
        
        if (float.IsNaN(Config.AdsTimer))
        {
            if (Config.DebugLogs == true) Logger.LogInformation($"AdsTimer setting to 60 seconds by default.");

            Config.AdsTimer = 60.0f;
        }
        
        var players = Utilities.GetPlayers().Where(x => x is { IsBot: false, Connected: PlayerConnectedState.PlayerConnected });
        
        foreach (var player in players)
        {
            if (player == null || Config == null || !player.IsValid || player.IsBot || player.IsHLTV) return;
            
            if (!IsGoldMember(player) && Config.ShowAds)
            {
                if (Config.DebugLogs == true)  Logger.LogInformation($"Displaying ad to non-GoldMember '{player.PlayerName}'.");

                player.PrintToChat(ReplaceColorPlaceholders(string.IsNullOrWhiteSpace(itemsString)
                    ? string.Format(Localizer["gold.BecomeGoldMemberMsgWithoutItems"], (object)string.Join(", ", Config.NameDns))
                    : string.Format(Localizer["gold.BecomeGoldMemberMsg"], (object)string.Join(", ", Config.NameDns), itemsString)));
                return;
            }
            if(Config.ShowAds)
            {
                if (Config.DebugLogs == true)  Logger.LogInformation($"Displaying ad to GoldMember '{player.PlayerName}'.");

                player.PrintToChat(ReplaceColorPlaceholders(string.IsNullOrWhiteSpace(itemsString)
                    ? Localizer["gold.IsGoldMemberMsgWithoutItems"]
                    : string.Format(Localizer["gold.IsGoldMemberMsg"], (object)itemsString)));
            }
        }
    }
    
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
    
    public HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null || player.IsBot || player.IsHLTV || !player.IsValid) return HookResult.Continue;
        
        if (player.IsValid) 
            AddTimer(Config.AdsTimer, PrintAds);
        
        return HookResult.Continue;
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
                    if (_api.IsPistolRound() && Config.GiveItemsDuringPistolRound == true && !_api.IsClientVip(player))
                    {
                        foreach (string item in Config.Items)
                        {
                            if ((player.TeamNum == 2 && item.Trim() == "weapon_molotov") || (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade"))
                                player.GiveNamedItem(item.Trim());
                            else
                                player.GiveNamedItem(item.Trim());

                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[PistolRound]");     
                        }
                    }
                    else if (!_api.IsPistolRound() && !_api.IsClientVip(player))
                    {
                        foreach (string item in Config.Items)
                        {
                            if ((player.TeamNum == 2 && item.Trim() == "weapon_molotov") || (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade"))
                                player.GiveNamedItem(item.Trim());
                            else
                                player.GiveNamedItem(item.Trim());
                            
                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[NormalRound]");    
                        }
                    }
                }
                
                if (_api.IsPistolRound() && Config.GiveMoney == true && Config.GiveMoneyInPistolRounds == true)
                {
                    var money = Config.Money;
                    moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);;
                    
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
                    moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);;
                    
                    if (moneyServices.Account > Config.MaxMoney) moneyServices.Account = Config.MaxMoney;
                    
                    if (Config.DebugLogs == true) 
                    {
                        Logger.LogInformation($"Gave {money}$ to {player.PlayerName}.[NormalRound]");
                        Logger.LogInformation($"Player {player.PlayerName} has {moneyServices.Account}$.[NormalRound]");
                    }
                }
                
                if (!_api.IsClientVip(player))
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
                
                if (!_api.IsClientVip(player) && Config.GiveVIPToPlayer  == true)
                {
                    if (Config.DebugLogs == true) Logger.LogInformation($"Gave VIP: {Config.VIPGroup} to player: {player.PlayerName}");   

                    _api.GiveClientTemporaryVip(player, Config.VIPGroup, Config.VIPTime != 0 ? Config.VIPTime : GetRoundTime());
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
                            if ((player.TeamNum == 2 && item.Trim() == "weapon_molotov") || (player.TeamNum == 3 && item.Trim() == "weapon_incgrenade"))
                                player.GiveNamedItem(item.Trim());
                            else
                                player.GiveNamedItem(item.Trim());

                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[PistolRound]");       
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

                            if (Config.DebugLogs == true) Logger.LogInformation($"Gave item: {item} to player: {player.PlayerName}.[NormalRound]");       
                        }
                    }
                }
                
                if (IsPistolRound() && Config.GiveMoney == true && Config.GiveMoneyInPistolRounds == true)
                {
                    var money = Config.Money;
                    moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);;
                    
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
                    moneyServices.Account = !Config.Money.Contains("++") ? int.Parse(Config.Money) : moneyServices.Account + int.Parse(Config.Money.Split("++")[1]);;
                    
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

            if(Config.SetClanTag == true && Config.ClanTag != null)
            {
                if (Config.DebugLogs == true) Logger.LogInformation($"Setting ClanTag for {player.PlayerName} to {Config.ClanTag}.");
                
                player.Clan = Config.ClanTag;
                Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
            }
        });
        return HookResult.Continue;
    }
}