# ArenaGunGame – Technical Documentation

## Overview

**ArenaGunGame** is a standalone Rust plugin that enforces a global weapon cycle in arena/sandbox mode. Zero dependencies on other plugins.

## Architecture

### Core Flow

```
OnServerInitialized
  ↓
  timer.Repeat(CycleInterval) → CycleWeapons()
    ├── AnnounceRoundStats()     [broadcast kill leader + KDA leader]
    ├── _roundStats.Clear()       [reset counter]
    ├── _weaponIndex++            [next weapon]
    └── GiveGlobalWeapon(player)  [all online players get new loadout]

OnPlayerRespawned / OnPlayerConnected
  ↓
  GiveGlobalWeapon(player)        [equip current loadout]

OnPlayerDeath
  ↓
  Track kill/death in _roundStats  [kills for attacker, deaths for victim]
```

### Global State

- **`_weaponIndex: int`** — Index into `Configuration.Loadouts[]`, incremented on each cycle. Wraps around: `(_weaponIndex + 1) % count`.
- **`_roundStats: Dictionary<ulong, RoundStats>`** — Keyed by player SteamID; persists even if player disconnects. Cleared at cycle boundary.

## Hooks & Interception

### Hook Signature & Purpose

| Hook | Called When | Purpose |
|---|---|---|
| `OnServerInitialized()` | Server finishes startup | Initialize `_weaponIndex`, start timer |
| `OnPlayerRespawned(BasePlayer)` | Player returns to life | Call `GiveGlobalWeapon` on next tick |
| `OnPlayerConnected(BasePlayer)` | Player joins | Immediately equip with current loadout |
| `OnPlayerDeath(BasePlayer, HitInfo)` | Player dies | Increment victim's `Deaths`, attacker's `Kills` |
| `OnEntityTakeDamage(BaseCombatEntity, HitInfo)` | Entity takes damage | Block TreeEntity, OreResourceEntity, LootContainer (scale damage to 0) |
| `CanLootEntity(BasePlayer, BaseEntity)` → object | Player attempts to open loot | Return `false` for corpses, bags, containers → deny |
| `CanLootPlayer(BasePlayer, BasePlayer)` → object | Player attempts to loot corpse | Always return `false` |

### Why These Hooks?

- **OnPlayerDeath**: Only place to reliably track kills/deaths in a round.
- **OnEntityTakeDamage**: Prevents environmental destruction. `ScaleAll(0f)` makes entity invulnerable without triggering drop logic.
- **CanLootEntity / CanLootPlayer**: Blocks looting at source, preventing item accumulation outside loadout.

## Data Structures

### `Loadout`
```csharp
Weapon: string                // shortname (e.g., "rifle.ak")
Attachments: List<string>     // weapon mods; placed in weapon.contents
Ammo: string                  // shortname (e.g., "ammo.rifle")
AmmoCount: int                // quantity
Wear: List<string>            // clothing/armor → containerWear
Extras: List<ItemStack>       // medical/food → containerMain
```

### `RoundStats`
```csharp
Name: string                  // player display name
Kills: int
Deaths: int
KDA: float                    // computed: Kills / max(Deaths, 1)
```

## GiveGlobalWeapon Flow

1. **Strip**: `player.inventory.Strip()` clears all containers
2. **Weapon**: Create item, place in `containerBelt`
3. **Attachments**: For each mod, create and move to `weapon.contents` via `MoveToContainer`
4. **Ammo**: Create stack, place in `containerMain`
5. **Wear**: Create each clothing item, place in `containerWear` (auto-equips to fitting slot)
   - If slot occupied, fallback to `containerMain`
6. **Extras**: Create each extra (medical, food), place in `containerMain`

### Item Containers

| Container | Slots | Purpose |
|---|---|---|
| `containerBelt` | 6 | Quick-access equip slot (weapon + gear) |
| `containerMain` | 24 | Primary backpack |
| `containerWear` | 7 | Clothing/armor (auto-assigns by category) |

Weapon attachments go in `weapon.contents` (item's internal storage).

## Cycle Mechanics

**Timer** fires every `CycleInterval` seconds (default 240s = 4 min):

1. Announce previous round stats (top killer, best KDA)
2. Clear `_roundStats`
3. Increment `_weaponIndex` and wrap
4. `GiveGlobalWeapon()` for all `BasePlayer.activePlayerList`
5. Broadcast new weapon name

**All players get new loadout simultaneously** — no individual kill-count tracking, purely global timer.

## Item Manager API

| Call | Returns | Purpose |
|---|---|---|
| `ItemManager.CreateByName(shortname, amount)` | Item | Create item by shortname. Returns `null` if invalid. |
| `item.MoveToContainer(container)` | bool | Move item into container; auto-assigns slot. `false` if no room. |
| `player.inventory.GiveItem(item, container)` | bool | Place item in container. Less strict than `MoveToContainer`. |
| `player.inventory.Strip()` | void | Empty all containers (belt, main, wear). |

## Kill Tracking Logic

```csharp
OnPlayerDeath(BasePlayer player, HitInfo info)
  player (victim) → Deaths++
  
  if (info?.InitiatorPlayer != null && initiator != victim)
    initiator → Kills++
  else
    // Environmental death, suicide → only victim.Deaths increments
```

**Self-kill is not counted as attacker's kill** (check `attacker.userID != player.userID`).

## Configuration

Config is JSON (`oxide/config/ArenaGunGame.json`), auto-generated first run.

```json
{
  "Cycle interval (seconds)": 240,
  "Loadouts": [
    {
      "label": "M249",
      "weapon": "lmg.m249",
      "attachments": ["weapon.mod.holosight", "weapon.mod.lasersight"],
      "ammo": "ammo.rifle",
      "ammo_count": 200,
      "wear": ["metal.facemask", "metal.plate.torso", ...],
      "extras": [
        {"shortname": "syringe.medical", "amount": 3},
        ...
      ]
    },
    ...
  ]
}
```

Modify to add/remove weapons, adjust ammo counts, or change armor.

## Unload

When plugin is unloaded: `_cycleTimer?.Destroy()` kills the recurring timer. No cleanup needed for dicts/stats (GC handles it).

## Performance Notes

- **No recurring queries**: Global state is in-memory integers/dicts.
- **Minimal hook overhead**: `OnEntityTakeDamage` fires per hit; `ScaleAll(0f)` is O(1).
- **GiveGlobalWeapon is O(n)** where n = loadout item count (~5–10 items per player).
- **AnnounceRoundStats uses LINQ** `.OrderByDescending()` once per cycle (acceptable).

---

## Common Pitfalls

1. **Weapon creation fails** → `ItemManager.CreateByName(invalidName, 1)` returns `null`. Config shortnames must be exact.
2. **Attachments don't attach** → `weapon.contents` might be `null` (non-weapon item). Guard with `if (weapon.contents != null)`.
3. **Wear item doesn't equip** → Slot occupied or item incompatible with category. Fallback to `containerMain` prevents item loss.
4. **KDA reports NaN** → Deaths = 0 case handled: `KDA = Kills / max(Deaths, 1)`.
5. **Stats lost on disconnect** → By design: keyed by SteamID, persists for round announcement.
