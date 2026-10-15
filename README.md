# 🥇 CS2 — GoldMember

> Reward players who carry your server **DNS / tag** in their nickname — free items, money, HP, armor, temporary VIP, and a custom scoreboard + chat tag.

<p>
  <img src="https://img.shields.io/badge/CounterStrikeSharp-%E2%89%A5%201.0.374-blue" alt="CSS version" />
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4" alt="dotnet" />
  <img src="https://img.shields.io/badge/version-0.3.0-gold" alt="version" />
</p>

---

## 📋 Requirements

| Dependency | Notes |
|---|---|
| **CounterStrikeSharp** | `>= 1.0.374` (.NET 10) |
| **Metamod:Source** | required |
| **VIPCore** | [partiusfabaa/cs2-VIPCore](https://github.com/partiusfabaa/cs2-VIPCore) — used for VIP perks / VIP checks |
| **ChatManager** | **required only if `SetTag` is enabled** — the shared broker that renders the scoreboard + chat tag |

> If `SetTag` is **on** but ChatManager is **not** installed, tags are simply not applied (no crash, no fallback).

---

## ⚙️ Installation

1. Drop the `addons/` folder into your server's `csgo/` directory (merge with CounterStrikeSharp).
2. Make sure the shared APIs live in `addons/counterstrikesharp/shared/` (`VipCoreApi`, `ChatManagerApi`) — **not** next to the plugin.
3. Start the server once — the config generates automatically.

```
addons/counterstrikesharp/
├─ plugins/GoldMember/      GoldMember.dll + lang/
├─ plugins/ChatManager/     ChatManager.dll        (if using tags)
└─ shared/VipCoreApi/ , shared/ChatManagerApi/
```

---

## 🎮 Commands

| Command | Access | Description |
|---|---|---|
| `!goldmember` | everyone | Show the benefits and how to become a GoldMember |
| `!gmlist` | everyone | List the GoldMembers currently online |
| `css_gm_reload` | `@css/root` | Reload the config live (no restart) |

---

## 🧩 Configuration

The config is generated automatically at
`addons/counterstrikesharp/configs/plugins/GoldMember/GoldMember.json`.

### Membership & restrictions
| Key | Example | Description |
|---|---|---|
| `NameDns` | `["dns1", "dns2"]` | Nickname must contain one of these to count as a GoldMember |
| `RestrictFlags` | `["@css/vip"]` | Players with any of these flags are skipped (no benefits) |
| `RestrictedCommands` | `["jointeam"]` | Commands usable **only** by GoldMembers |

### Benefits
| Key | Example | Description |
|---|---|---|
| `GiveItems` / `Items` | `true` / `["weapon_hegrenade"]` | Free items on spawn |
| `GiveItemsDuringPistolRound` | `false` | Also give items on pistol rounds |
| `GiveHealth` / `Health` | `true` / `100` | Set spawn HP |
| `GiveArmor` / `Armor` | `true` / `100` | Set spawn armor |
| `GiveMoney` / `Money` | `true` / `"16000"` | Set money. `"++3000"` **adds** 3000 instead of setting |
| `GiveMoneyInPistolRounds` | `false` | Also give money on pistol rounds |
| `MaxMoney` | `16000` | Cap applied after giving money |

### VIP (via VIPCore)
| Key | Example | Description |
|---|---|---|
| `VipCoreEnabled` | `true` | Integrate with VIPCore (skips benefits for existing VIPs so they don't stack) |
| `GiveVIPToPlayer` | `false` | Grant temporary VIP to GoldMembers |
| `VIPGroup` | `"gold"` | VIP group to grant |
| `VIPTime` | `2` | Duration (VIPCore TimeMode). `0` = one round |

### 🏷️ Tags (ChatManager)
One toggle sets **both** the scoreboard clan tag and the chat tag:

```jsonc
"SetTag": true,
"Tag": {
  "ClanTag": "GoldMember®",        // scoreboard tag (plain text)
  "ChatTag": {
    "Tag": "[GoldMember]",         // chat prefix text
    "TagColor": "{gold}",          // prefix colour
    "NameColor": "{lime}",         // player name colour
    "MessageColor": "{default}"    // message colour
  }
}
```

> Tags are rendered through **ChatManager**, so they combine cleanly with VIP and Admin tags
> (order: `VIP → GoldMember → Admin`).

### Advertisements & misc
| Key | Example | Description |
|---|---|---|
| `ShowAds` | `true` | Periodic chat ad (different text for members vs non-members) |
| `AdsTimer` | `60` | Ad interval in seconds |
| `WelcomeMessage` | `true` | Private welcome message for members on join |
| `AnnounceJoinLeave` | `true` | Announce member join/leave in chat |
| `AdminReloadPermission` | `"@css/root"` | Flag required for `css_gm_reload` |
| `DebugLogs` | `false` | Verbose console logging |

---

## 🎨 Supported colors

`{default}` `{white}` `{darkred}` `{green}` `{lightyellow}` `{lightblue}` `{olive}`
`{lime}` `{red}` `{lightpurple}` `{purple}` `{grey}` `{yellow}` `{gold}` `{silver}`
`{blue}` `{darkblue}` `{bluegrey}` `{magenta}` `{lightred}` `{orange}`

---

## 🌍 Languages

Ships with **English**, **Romanian** and **Russian** (`lang/en.json`, `ro.json`, `ru.json`).
Add your own `lang/<code>.json` with the same keys for more.
