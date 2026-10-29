using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace GoldMember;

public class GoldMemberConfig : BasePluginConfig
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
    [JsonPropertyName("SetTag")]
    public bool SetTag { get; set; } = false;
    [JsonPropertyName("Tag")]
    public TagConfig Tag { get; set; } = new TagConfig();
    [JsonPropertyName("ShowAds")]
    public bool ShowAds { get; set; } = true;
    [JsonPropertyName("AdsTimer")]
    public float AdsTimer { get; set; } = 60.0f;
    [JsonPropertyName("WelcomeMessage")]
    public bool WelcomeMessage { get; set; } = true;
    [JsonPropertyName("AnnounceJoinLeave")]
    public bool AnnounceJoinLeave { get; set; } = true;
    [JsonPropertyName("AdminReloadPermission")]
    public string AdminReloadPermission { get; set; } = "@css/root";
    [JsonPropertyName("RestrictedCommands")]
    public List<string> RestrictedCommands { get; set; } = new List<string>();
    [JsonPropertyName("DebugLogs")]
    public bool DebugLogs { get; set; } = false;
    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 14;
}

public class TagConfig
{
    [JsonPropertyName("ClanTag")]
    public string ClanTag { get; set; } = "GoldMember®";

    [JsonPropertyName("ChatTag")]
    public ChatTagConfig ChatTag { get; set; } = new ChatTagConfig();
}

public class ChatTagConfig
{
    // Prefix shown before the name in chat (may use {color} tokens, but TagColor is simpler).
    [JsonPropertyName("Tag")]
    public string Tag { get; set; } = "[GoldMember]";

    // Colour of the tag prefix.
    [JsonPropertyName("TagColor")]
    public string TagColor { get; set; } = "{gold}";

    // Colour of the player's name in chat.
    [JsonPropertyName("NameColor")]
    public string NameColor { get; set; } = "{lime}";

    // Colour of the message body in chat.
    [JsonPropertyName("MessageColor")]
    public string MessageColor { get; set; } = "{default}";
}
