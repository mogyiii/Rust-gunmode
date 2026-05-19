# ArenaGunGame – Rust Plugin

**Standalone arena-mode plugin** for Rust with global weapon cycling, kill tracking, and sandbox environment.

## Features

✅ **Global Weapon Cycle** – Timer-based, not kill-based. All players switch weapons simultaneously.  
✅ **Round Statistics** – Announces round MVP (most kills) and best KDA at cycle end.  
✅ **Sandbox Environment** – Trees, ores, barrels, crates are indestructible. No loot from corpses or bags.  
✅ **Full Loadouts** – Each weapon includes armor, attachments, ammo, medical items.  
✅ **Zero Dependencies** – Standalone; no reliance on anti-cheat, event managers, or other plugins.  
✅ **Configurable** – Easy JSON config to add/remove weapons, adjust cycle time, rebalance ammo.  
✅ **Graceful Spawns** – Players respawn with current weapon and empty inventory each round.

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

| Command | Effect |
|---|---|
| `/oxide.reload ArenaGunGame` | Reload config (no server restart) |
| `/oxide.unload ArenaGunGame` | Stop plugin, kill timer |

---

## Game Flow

```
1. Server starts
   ↓
2. Players join → each gets current loadout
   ↓
3. Fight for X seconds (default 240 = 4 min)
   ↓
4. Timer fires
   ├── Announce round results (kill leader, KDA leader)
   ├── Clear round stats
   ├── Advance to next weapon
   └── All players get new loadout
   ↓
5. Repeat step 3
```

---

## Loadouts Included

| # | Weapon | Armor Tier | Ammo | Note |
|---|---|---|---|---|
| 1 | M249 | Heavy (metal) | 200x rifle | Suppression, many mods |
| 2 | AK-47 | Heavy (metal) | 120x rifle | Standard assault |
| 3 | LR-300 | Heavy (metal) | 120x rifle | Scoped assault |
| 4 | SPAS-12 | Medium (coffee) | 64x shotgun | Close combat |
| 5 | Pump Shotgun | Light (coffee) | 32x slug | Slug rounds |
| 6 | MP5 | Light (hoodie) | 200x pistol | SMG + silencer |
| 7 | Thompson | Light (hoodie) | 180x pistol | Mid SMG |
| 8 | Semi-Auto Rifle | Light (hoodie) | 100x rifle | Scoped mid-range |
| 9 | Python | Minimal (t-shirt) | 48x pistol | Revolver |
| 10 | Bolt Action | Light (coffee) | 30x rifle (HV) | Sniper, 8x scope |
| 11 | Crossbow | Minimal (t-shirt) | 30x wood arrow | Primitive last |

Each loadout includes **healing items** (syringes, bandages) scaled to weapon type.

---

## Configuration

**File**: `oxide/config/ArenaGunGame.json`

### Modify Cycle Time

```json
{
  "Cycle interval (seconds)": 60
}
```

Changes to 1-minute rounds.

### Add a New Loadout

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

### Round Start (normal)
```
[GunGame] Следующий weapon: <color>#ffcc00>M249</color>
```

### Round End (with kills)
```
[GunGame] <color=#aaffaa>Legtöbb kill:</color> <color=#ffff00>Player A</color> — 8K / 2D
[GunGame] <color=#aaffaa>Legjobb KDA:</color> <color=#ffff00>Player B</color> — 5K / 1D (5.00)
```

---

## Architecture

### Core Hooks

- **`OnServerInitialized`** – Start timer
- **`OnPlayerRespawned`** – Give current weapon on respawn
- **`OnPlayerConnected`** – Give current weapon on join
- **`OnPlayerDeath`** – Track kills/deaths for stats
- **`OnEntityTakeDamage`** – Block tree/ore/barrel damage
- **`CanLootEntity`** – Block corpse/bag/container looting

### Global State

- **`_weaponIndex`** (int) – Current weapon in cycle
- **`_roundStats`** (dict) – Kills/deaths per player per round
- **`_cycleTimer`** (Timer) – Rust timer object firing each cycle

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

### Stats not announcing

**Cause**: No kills that round (all deaths environmental).

**Fix**: Normal; only announces if `Kills > 0` for someone.

### Plugin won't load

**Cause**: Syntax error in `ArenaGunGame.cs` or config JSON.

**Fix**: Check server console for error. Verify JSON syntax at [jsonlint.com](https://www.jsonlint.com/).

---

## Design Philosophy

1. **Standalone** – No dependencies on other plugins
2. **Simple** – Global state, minimal hooks, clear flow
3. **Balanced** – Weapon progression from heavy (M249) to primitive (crossbow)
4. **Configurable** – All weapons, ammo, armor, cycle time customizable
5. **Informative** – Round stats broadcast give players feedback

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

- **Version**: 1.0.0
- **Author**: dev
- **Rust Version**: Current (compatible with recent Rust updates)

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
