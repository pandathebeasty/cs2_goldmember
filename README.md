# CS2 plugin - GoldMember
## Receiving benefits for having DNS in nickname

## Requirements
*(OPTIONAL)* **VIPCore**:  [partiusfabaa/cs2-VIPCore](https://github.com/partiusfabaa/cs2-VIPCore) => minimum version: ***1.2.7***\
**Minimum CounterStrikeSharp Version**: ***246***

## Config is generated automatically by plugin
- **NameDns**: ["dns1","dns2", etc]
- **RestrictFlags**: ["@css/vip"] // if player has flags then plugin will not give anything
- **GiveItems**: true //give items to player
- **GiveItemsDuringPistolRound**: true //will give items in PistolRound, only if GiveItems: true
- **Items**: ["item_name", etc]
- **GiveHealth**: true //will give health to player
- **Health**: health_value // ex Health: 100
- **GiveArmor**: true //will give armor to player
- **Armor**: armor_value //ex Armor:100
- **GiveMoney**: true //will give money to player
- **Money**: "16000" // if "++3000" will add 3000 to current money value
- **MaxMoney**: 16000 //maximum amount of money that a player can have
- **VipCoreEnabled**: true // if VipCore is on server
- **GiveVIPToPlayer**: true // will give VIP to player
- **VIPGroup**: "vip_group" // which group to give if **GiveVIPToPlayer** is true
- **VIPTime**: time // depends on TimeMode from VIPCore, if 0 then will give VIP for RoundTime
- **SetClanTag**: true // will set ClanTag to player
- **ClanTag** : "[GoldMember]"
- **ShowAds** : true // either to show or not the ads
- **AdsTimer** : 60 // interval of the ads
- **DebugLogs**: false //if true will create logs
- **RestrictedCommands**: [] // which commands to be restricted to Gold Members only

## Supported colors
- {default}
- {white}
- {darkred}
- {green}
- {lightyellow}
- {lightblue}
- {olive}
- {lime}
- {red}
- {lightpurple}
- {purple}
- {grey}
- {yellow}
- {gold}
- {silver}
- {blue}
- {darkblue}
- {bluegrey}
- {magenta}
- {lightred}
- {orange}
