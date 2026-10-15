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

namespace GoldMember;

/*
 * Gold Member - grants benefits to players who carry a configured DNS/tag in their name.
 *
 * The implementation is split across partial-class files by responsibility:
 *   GoldMember.cs         - plugin metadata, fields, lifecycle (load / config)
 *   GoldMemberConfig.cs   - the config model
 *   GoldMember.Helpers.cs - shared helpers (membership check, money/item resolve, colors, round)
 *   GoldMember.Benefits.cs- OnPlayerSpawn: grants items / money / health / armor / VIP / clan tag
 *   GoldMember.Ads.cs     - periodic advertisement messages
 *   GoldMember.Commands.cs- restricted-command handling
 */
[MinimumApiVersion(374)]
public partial class GoldMember : BasePlugin, IPluginConfig<GoldMemberConfig>
{
    public override string ModuleName => "Gold Member";
    public override string ModuleVersion => "0.3.0";
    public override string ModuleAuthor => "panda (Discord: panda.4179)";
    public override string ModuleDescription => "Benefits for those who have DNS in name (https://github.com/pandathebeasty/cs2_goldmember)";

    public GoldMemberConfig Config { get; set; } = new GoldMemberConfig();

    public IVipCoreApi? _api;
    public PluginCapability<IVipCoreApi> PluginCapability { get; } = new("vipcore:core");

    public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
    public static readonly string CfgPath = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "plugins", AssemblyName, $"{AssemblyName}.json");

    private CounterStrikeSharp.API.Modules.Timers.Timer? _adsTimer;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        // Player-facing info + list commands (anyone), and an admin-only config reload.
        AddCommand("css_goldmember", "Shows the GoldMember benefits and how to become one.", OnGoldMemberCommand);
        AddCommand("css_gmlist", "Lists the GoldMembers currently online.", OnGmListCommand);
        AddCommand("css_gm_reload", "Reloads the GoldMember config (admin only).", OnReloadCommand);

        if (Config.DebugLogs == true)
        {
            Logger.LogInformation("\u001b[1;32mEvents \u001b[1;33m~OnPlayerSpawn/ConnectFull/Disconnect~ \u001b[1;32mregistered!");
        }

        // Single, repeating ads timer for the whole server. The old code started one timer PER
        // player connect, without REPEAT - that fired once per connect and PrintAds already loops
        // over everyone, so N players meant N overlapping one-shot broadcasts (spam). One global
        // repeating timer is correct.
        if (Config.ShowAds)
        {
            var interval = (Config.AdsTimer is float.NaN or <= 0f) ? 60.0f : Config.AdsTimer;
            _adsTimer = AddTimer(interval, PrintAds, TimerFlags.REPEAT);
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

        InitChatTag();
    }

    public override void Unload(bool hotReload)
    {
        ShutdownChatTag();
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
