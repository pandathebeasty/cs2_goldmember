using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VipCoreApi;
using VipCoreApi.Args;
using VipCoreApi.Enums;


namespace GoldMember;

[MinimumApiVersion(374)]
public partial class GoldMember : BasePlugin, IPluginConfig<GoldMemberConfig>
{
    public override string ModuleName => "Gold Member";
    public override string ModuleVersion => "0.3.1";
    public override string ModuleAuthor => "panda (Discord: panda.4179)";
    public override string ModuleDescription => "Benefits for those who have DNS in name (https://github.com/pandathebeasty/cs2_goldmember)";

    public GoldMemberConfig Config { get; set; } = new GoldMemberConfig();

    private IVipCoreApi? _api;
    
    public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
    public static readonly string CfgPath = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", AssemblyName, $"{AssemblyName}.json");

    private CounterStrikeSharp.API.Modules.Timers.Timer? _adsTimer;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        
        AddCommand("css_goldmember", "Shows the GoldMember benefits and how to become one.", OnGoldMemberCommand);
        AddCommand("css_gmlist", "Lists the GoldMembers currently online.", OnGmListCommand);
        AddCommand("css_gm_reload", "Reloads the GoldMember config (admin only).", OnReloadCommand);

        if (Config.DebugLogs == true)
        {
            Logger.LogInformation("\u001b[1;32mEvents \u001b[1;33m~OnPlayerSpawn/ConnectFull/Disconnect~ \u001b[1;32mregistered!");
        }
        
        if (Config.ShowAds)
        {
            var interval = (Config.AdsTimer is float.NaN or <= 0f) ? 60.0f : Config.AdsTimer;
            _adsTimer = AddTimer(interval, PrintAds, TimerFlags.REPEAT);
        }

        if (Config.VipCoreEnabled)
        {
            _api = IVipCoreApi.Capability.Get();
            
            if (_api == null)
            {
                Logger.LogWarning("VIPCore was not found.");
                return;
            }
            
            if (Config.DebugLogs)
                Logger.LogInformation("\u001b[1;32mRunning with \u001b[1;33mVIPCore\u001b[1;32m!");
        }
        else
        {
            if (Config.DebugLogs)
                Logger.LogInformation("\u001b[1;33mRunning without \u001b[1;31mVIPCore\u001b[1;33m!");
        }

        foreach (string command in Config.RestrictedCommands)
        {
            AddCommandListener($"{command}", (player, args) => RestrictCommands(player, command));
        }
        
        if (Config.SetTag)
            InitTags();
    }

    public override void Unload(bool hotReload)
    {
        if (Config.SetTag)
            ShutdownTag();
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
}
