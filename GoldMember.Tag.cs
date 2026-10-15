using ChatManagerApi;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Microsoft.Extensions.Logging;

namespace GoldMember;

public partial class GoldMember
{
    // GoldMember sits between VIP and admin in the chat line: its prefix comes after VIP (Order 10)
    // but before admin (Order 20), and it wins colours over VIP but loses to admin.
    private const int TagOrder = 15;
    private const int TagColorPriority = 75;
    private const string ChatProviderName = "goldmember";

    private readonly PluginCapability<IChatManagerApi?> _chatManagerCap = IChatManagerApi.Capability;
    private IChatManagerApi? _chatManager;

    /// <summary>
    /// Resolves ChatManager and registers GoldMember as a tag provider. Called from
    /// OnAllPluginsLoaded. If SetTag is on but ChatManager is missing, tags simply do not apply
    /// (hard dependency, by design - no direct clan-tag fallback).
    /// </summary>
    public void InitChatTag()
    {
        if (!Config.SetTag)
        {
            return;
        }

        try { _chatManager = _chatManagerCap.Get(); } catch { _chatManager = null; }

        if (_chatManager == null)
        {
            Logger.LogWarning("[GoldMember] SetTag is enabled but ChatManager was not found - tags will not be applied. Install the ChatManager plugin + shared ChatManagerApi.dll.");
            return;
        }

        _chatManager.RegisterProvider(ChatProviderName, GetChatTag);
        if (Config.DebugLogs) Logger.LogInformation("[GoldMember] Registered as a ChatManager tag provider.");
    }

    public void ShutdownChatTag()
    {
        _chatManager?.UnregisterProvider(ChatProviderName);
    }

    /// <summary>Refreshes a player's tag in ChatManager (chat line + scoreboard) immediately.</summary>
    public void RefreshChatTag(CCSPlayerController player)
    {
        _chatManager?.RefreshTags(player);
    }

    /// <summary>
    /// Provider callback: returns the GoldMember tag for a player, or null when they are not a
    /// GoldMember (so ChatManager adds nothing for them).
    /// </summary>
    private ChatTag? GetChatTag(CCSPlayerController player)
    {
        if (!Config.SetTag || !IsGoldMember(player))
        {
            return null;
        }

        var t = Config.Tag;
        return new ChatTag
        {
            // Scoreboard clan tag (plain text; ChatManager strips any stray tokens).
            ScoreboardTag = t.ClanTag,
            // Chat prefix = colour + tag text, e.g. "{gold}[GoldMember]".
            ChatPrefix = $"{t.ChatTag.TagColor}{t.ChatTag.Tag}",
            NameColor = t.ChatTag.NameColor,
            MessageColor = t.ChatTag.MessageColor,
            Order = TagOrder,
            ColorPriority = TagColorPriority
        };
    }
}
