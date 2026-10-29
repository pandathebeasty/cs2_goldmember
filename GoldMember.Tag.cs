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
    
    public void InitTags()
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

        _chatManager.RegisterProvider(ChatProviderName, GetTag);
        if (Config.DebugLogs) Logger.LogInformation("[GoldMember] Registered as a ChatManager tag provider.");
    }
    
    public void ShutdownTag()
    {
        _chatManager?.UnregisterProvider(ChatProviderName);
    }
    
    public void RefreshTag(CCSPlayerController player)
    {
        _chatManager?.RefreshTags(player);
    }
    
    private ChatTag? GetTag(CCSPlayerController player)
    {
        if (!Config.SetTag || !IsGoldMember(player))
        {
            return null;
        }

        var t = Config.Tag;
        return new ChatTag
        {
            ScoreboardTag = t.ClanTag,
            ChatPrefix = $"{t.ChatTag.TagColor}{t.ChatTag.Tag}",
            NameColor = t.ChatTag.NameColor,
            MessageColor = t.ChatTag.MessageColor,
            Order = TagOrder,
            ColorPriority = TagColorPriority
        };
    }
}