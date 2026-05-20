# ArenaGunGame – Rust Plugin

**Standalone arena-mode plugin** for Rust with two game modes: Gun Game (weapon cycling) and King of the Hill (zone-based scoring).

## Features

### Gun Game Mode
✅ **Global Weapon Cycle** – Timer-based (default 4 min). All players switch weapons simultaneously.  
✅ **18 Loadouts** – Full progression from heavy assault (M249) to primitive (Compound Bow).  
✅ **Kill Tracking** – Round leaderboard by kills and KDA.  

### King of the Hill Mode
✅ **Dynamic Zone** – Random map position, rotates every 3 minutes.  
✅ **Zone Scoring** – +10 points/sec in zone, +100 points per kill in zone.  
✅ **Live Leaderboard** – `/stats` shows Top 5 players in real-time.  
✅ **Visual Map Marker** – Orange circle on in-game map marks zone location.  

### Shared Features
✅ **Random Terrain-Based Spawns** – 60 spawn points across the map, no more center camp.  
✅ **Vehicle Spawning** – `/v car|bike|moto` with 5-minute cooldown.  
✅ **Sandbox Environment** – Trees, ores, barrels are indestructible. No loot from corpses.  
✅ **Full Loadouts** – Armor, attachments, ammo, healing items per weapon.  
✅ **Configurable** – JSON config for weapons, modes, timers, vehicle prefabs.  
✅ **Zero Dependencies** – Standalone; no reliance on other plugins.

---

## Quick Start

### Installation

1. **Drop plugin** in `oxide/plugins/`:
   ```
   oxide/plugins/ArenaGunGame.cs
   ```

2. **Load on server start** (automatic) or manually:
   ```
   /oxide.load ArenaGunGame
   ```

3. **First run** generates config:
   ```
   oxide/config/ArenaGunGame.json
   ```

### Default Behavior

- **Cycle interval**: 4 minutes (240 seconds)
- **11 weapons** rotate: M249 → AK → LR-300 → ... → Crossbow
- **Rounds announce** top killer and best KDA (K/D ratio)
- **Weapons + ammo + armor** given automatically

---

## Commands

### Admin Commands
| Command | Effect |
|---|---|
| `/gg help` | Show admin commands |
| `/gg mode gungame` | Switch to Gun Game mode |
| `/gg mode koth` | Switch to King of the Hill mode |
| `/gg hill` | Display current KoTH zone location (grid + coordinates) |
| `/gg spawn` | Regenerate 60 spawn points |

### Player Commands
| Command | Effect |
|---|---|
| `/stats` | Live leaderboard – Top 5 by kills (GunGame) or zone points (KoTH) |
| `/v car` | Spawn modular car (5 min cooldown) |
| `/v bike` | Spawn dirt bike/motorbike (5 min cooldown) |
| `/v moto` | Spawn motorbike with sidecar (5 min cooldown) |

### Oxide Framework
| Command | Effect |
|---|---|
| `/oxide.reload ArenaGunGame` | Reload config (no server restart) |
| `/oxide.unload ArenaGunGame` | Stop plugin, kill timers |

---

## Game Flow

### Gun Game Mode
```
1. Server starts (Mode = "GunGame")
   ↓
2. Players join → spawn at random terrain-based location, get current weapon
   ↓
3. Fight for X seconds (default 240 = 4 min)
   ↓
4. Cycle timer fires
   ├── Announce round results (Top 3 by kills/KDA)
   ├── Clear stats, cleanup dropped items
   ├── Advance to next weapon (M249 → AK → LR-300 → ... → Compound Bow)
   └── All players respawn, get new loadout
   ↓
5. Repeat step 3
```

### King of the Hill Mode
```
1. Server starts (Mode = "KingOfTheHill")
   ↓
2. Zone spawns at random map position, orange marker appears
   ↓
3. Players join → spawn at random location, get current loadout
   ↓
4. Zone is active for 180 seconds (3 min)
   ├── Players in zone: +10 pts/sec
   ├── Players who kill in zone: +100 pts
   ├── Zone marker visible on in-game map
   ↓
5. Zone rotates to new random position
   ├── Announce new zone location with grid reference + coordinates
   └── Repeat step 4
   ↓
6. At weapon cycle time (default 4 min)
   ├── Announce Top 3 by zone points
   ├── Clear stats, cleanup items
   └── Repeat step 5
```

---

## Loadouts Included (18 Total)

### Heavy Armor Tier (metal.facemask + metal.plate.torso)
| # | Weapon | Ammo | Special Items |
|---|---|---|---|
| 1 | M249 | 2000x rifle | Holosight, laser, NVG, F1 grenades ×2 |
| 2 | AK-47 | 1200x rifle | Holosight, muzzle brake, NVG, F1 grenades ×2 |
| 3 | LR-300 | 1200x rifle | Scope, muzzle brake, NVG, F1 grenades ×2 |
| 12 | L96 | 120x rifle | NVG, F1 grenades ×2 |

### Medium Armor Tier (coffeecan.helmet)
| # | Weapon | Ammo | Special Items |
|---|---|---|---|
| 4 | SPAS-12 | 64x shotgun | Torch, beancan grenades ×2 |
| 5 | Pump Shotgun | 64x slug | Torch, beancan grenades ×2 |
| 10 | Bolt Action | 60x rifle (HV) | Torch, beancan grenades ×2 |
| 13 | M39 Rifle | 150x rifle | Torch, beancan grenades ×2 |

### Light Armor Tier (hoodie or jacket)
| # | Weapon | Ammo | Special Items |
|---|---|---|---|
| 6 | MP5 | 200x pistol | Torch, beancan grenades ×2 |
| 7 | Thompson | 180x pistol | Torch, beancan grenades ×2 |
| 8 | Semi-Auto Rifle | 100x rifle | Torch, beancan grenades ×2 |
| 14 | Custom SMG | 200x pistol | Torch, beancan grenades ×2 |
| 17 | Compound Bow | 30x wood arrow | Torch, beancan grenades ×2 |

### Minimal Armor Tier (t-shirt)
| # | Weapon | Ammo | Special Items |
|---|---|---|---|
| 9 | Python | 48x pistol | Flashlight.held, beancan grenades ×2 |
| 11 | Crossbow | 30x wood arrow | Torch, beancan grenades ×2 |
| 15 | Revolver | 48x pistol | Flashlight.held, beancan grenades ×2 |
| 16 | Semi-Auto Pistol | 72x pistol | Flashlight.held, beancan grenades ×2 |
| 18 | Double Barrel | 16x slug | Flashlight.held, beancan grenades ×2 |

**All loadouts include**: binoculars (belt), can.beans ×2, bandage ×10, syringe.medical ×2-4, lowgradefuel ×30

---

## Configuration

**File**: `oxide/config/ArenaGunGame.json`

### Core Settings

```json
{
  "Cycle interval (seconds)": 240,
  "Game mode (GunGame / KingOfTheHill)": "GunGame"
}
```

### King of the Hill Settings

```json
{
  "KoTH: zone radius (meters)": 150,
  "KoTH: hill rotate interval (seconds)": 180,
  "KoTH: zone points per second (in zone)": 10,
  "KoTH: points per kill": 100,
  "KoTH: tick interval (seconds)": 1
}
```

- **KothRadius**: Size of the active zone (150m = radius around center point)
- **KothRotateInterval**: How long before zone moves to a new location (180s = 3 min)
- **KothZonePointsPerSec**: Points players earn per second while inside the zone
- **KothKillPoints**: Bonus points for kills scored inside the zone

### Vehicle Settings

```json
{
  "Vehicle: spawn cooldown (seconds)": 300,
  "Vehicle: car prefab": "assets/content/vehicles/modularcar/modular_car.entity.prefab",
  "Vehicle: bike prefab": "assets/content/vehicles/motorbike/motorbike.entity.prefab",
  "Vehicle: motorbike prefab": "assets/content/vehicles/motorbike/motorbike_sidecar.entity.prefab"
}
```

- **VehicleCooldown**: Seconds between each player's vehicle spawns (shared across all vehicle types)
- **Prefab paths**: Configurable for server compatibility (copy exact values from your Rust version)

### Modify Cycle Time

Changes `Cycle interval (seconds)` from `240` to `60` for 1-minute rounds.

### Add a New Loadout

Add a loadout object to the `Loadouts` array:

```json
{
  "label": "Custom Rifle",
  "weapon": "rifle.ak",
  "attachments": ["weapon.mod.holosight"],
  "ammo": "ammo.rifle",
  "ammo_count": 150,
  "wear": ["hoodie", "pants", "shoes.boots"],
  "extras": [
    {"shortname": "bandage", "amount": 5}
  ]
}
```

### Remove a Loadout

Delete the loadout object from the `Loadouts` array and reload.

**→ Full reference**: See `CONFIGURATION.md`

---

## Server Console Output

### Gun Game Mode

**Round Start**:
```
[GunGame] Next weapon: M249
```

**Round End**:
```
[GunGame] ══ Round Over ══
[GunGame] #1 Player A — 8K / 2D | KDA: 4.00
[GunGame] #2 Player B — 6K / 3D | KDA: 2.00
[GunGame] #3 Player C — 5K / 4D | KDA: 1.25
```

### King of the Hill Mode

**Zone Rotation**:
```
[KoTH] New zone: G3 (525.2, -180.4) — step inside to earn points! (Map: orange circle)
```

**Zone Activity** (player messages):
```
[KoTH] You entered the zone! +10 pts/sec
[KoTH] You left the zone!
[KoTH] +100 pts! Total: 1250
```

**Round End**:
```
[KoTH] ══ Round Over – Leaderboard ══
[KoTH] #1 Player A — 850 pts (12K · 3D)
[KoTH] #2 Player B — 620 pts (9K · 5D)
[KoTH] #3 Player C — 475 pts (6K · 7D)
```

### Spawn System

**Initialization**:
```
[GunGame] Generated 60 spawn points (World.Size=1500).
```

---

## Architecture

### Core Hooks

- **`OnServerInitialized`** – Generate spawn points, start weapon cycle, optionally start KoTH zone rotation
- **`OnPlayerRespawned`** – Teleport to random spawn point, give current weapon
- **`OnPlayerConnected`** – Give current weapon + armor on join
- **`OnPlayerDeath`** – Track kills/deaths; add zone points if kill occurred in KoTH zone
- **`OnEntityTakeDamage`** – Block all damage to trees/ores/barrels/containers
- **`CanLootEntity`** – Block looting (except vehicle fuel tanks)
- **`CanLootPlayer`** – Block player-on-player looting

### Global State

- **`_weaponIndex`** (int) – Current weapon in cycle
- **`_activeMode`** (GameMode) – GunGame or KingOfTheHill
- **`_roundStats`** (dict) – Player stats (kills, deaths, KothScore) per round
- **`_spawnPoints`** (list) – 60 random terrain-based spawn locations
- **`_hillPosition`** (Vector3) – Current KoTH zone center (updated every rotation)
- **`_hillMarker`** (MapMarkerGenericRadius) – Orange map circle marking the zone
- **`_kothRotateTimer`** (Action) – Timer for zone rotation (3 min default)
- **`_kothTickTimer`** (Action) – Timer for zone point accumulation (1s tick)
- **`_vehicleCooldowns`** (dict) – Per-player vehicle spawn cooldown tracking

**→ Full details**: See `TECHNICAL.md`

---

## Testing

The plugin includes comprehensive test documentation.

**→ Test guide**: See `TESTING.md`

**Quick self-test**:
1. Connect to server
2. Check inventory for weapon + ammo + armor
3. Fight for cycle time
4. At cycle end, verify broadcast and new weapon
5. Die and respawn → get new weapon immediately

---

## Troubleshooting

### Players spawn without weapon

**Cause**: Invalid weapon shortname in config.

**Fix**: Check `oxide/config/ArenaGunGame.json` for typos. Verify shortname against [Rust item list](https://rustlabs.com/).

### Attachments don't appear on weapon

**Cause**: Attachment shortname invalid or incompatible with weapon.

**Fix**: Verify attachment shortname. Weapon still equips; attachment is silently skipped.

### Vehicle spawn fails ("Failed to spawn vehicle")

**Cause**: Invalid prefab path in config (vehicle entity doesn't exist in this Rust version).

**Fix**: Check server console log for `[GunGame] Vehicle spawn failed — prefab not found: ...`. Update the prefab paths in `oxide/config/ArenaGunGame.json` to match your Rust version.

**Known prefab paths** (for reference):
- Car: `assets/content/vehicles/modularcar/modular_car.entity.prefab`
- Bike: `assets/content/vehicles/motorbike/motorbike.entity.prefab`
- Moto: `assets/content/vehicles/motorbike/motorbike_sidecar.entity.prefab`

### Zone doesn't rotate or is missing in KoTH mode

**Cause**: `KothRotateInterval` set to 0, or zone marked as "destroyed".

**Fix**: Ensure `KothRotateInterval` is > 0 (default 180). Check server console for errors. Restart plugin via `/oxide.reload ArenaGunGame`.

### Stats showing on only one player

**Cause**: `/stats` command not being called; only round-end broadcasts visible.

**Fix**: Normal; use `/stats` to see live leaderboard. Round-end announces show top 3-5 at weapon cycle time.

### Plugin won't load

**Cause**: Syntax error in `ArenaGunGame.cs` or config JSON.

**Fix**: Check server console for error. Verify JSON syntax at [jsonlint.com](https://www.jsonlint.com/).

---

## Design Philosophy

1. **Standalone** – No dependencies on other plugins
2. **Simple** – Global state, minimal hooks, clear flow
3. **Flexible** – Two distinct game modes (Gun Game and King of the Hill) selectable at runtime
4. **Balanced** – Weapon progression from heavy (M249) to primitive (crossbow); zone scoring balanced against kills
5. **Configurable** – All weapons, ammo, armor, timers, modes, vehicle prefabs customizable
6. **Informative** – Round stats and live leaderboards give players feedback; zone location announced with grid reference

## Map Grid System

Positions are displayed using **Rust's grid system** — letters (A–J on a 1500m map) for columns, numbers for rows:
- **Grid reference**: Combination of column letter and row number (e.g., `G3`)
- **Column letters**: A (west) → J (east)
- **Row numbers**: 1 (north) → 10 (south)
- **Cell size**: ~150m per cell (varies with world size)

Example: Zone announces "`New zone: G3 (525.2, -180.4)`"
- `G3` – Grid reference (used to navigate on map)
- `(525.2, -180.4)` – World coordinates for precise pinning

---

## Files

| File | Purpose |
|---|---|
| `ArenaGunGame.cs` | Plugin source code |
| `README.md` | This file; overview & quick start |
| `TECHNICAL.md` | Deep dive: hooks, architecture, data structures |
| `TESTING.md` | Test scenarios and acceptance criteria |
| `CONFIGURATION.md` | Config reference and customization guide |

---

## Requirements

- **Rust Server** (Oxide/Carbon mod loader)
- **Oxide or Carbon framework** (plugin uses standard Oxide API)
- **.NET Framework 4.7+** (standard on servers)

---

## Version

- **Version**: 1.1.0
- **Author**: Mogy
- **Last Updated**: 2026-05-20
- **Rust Version**: Current (compatible with recent Rust updates)
- **Framework**: Carbon/Oxide (standard Oxide API)

---

## License

MIT (free to use, modify, redistribute)

---

## Support

For issues, customization, or questions:

1. **Check** `TECHNICAL.md` for architecture details
2. **Check** `CONFIGURATION.md` for config syntax
3. **Check** `TESTING.md` for expected behavior
4. **Check** server console for error logs

---

**Enjoy your Arena GunGame rounds!** 🎮
