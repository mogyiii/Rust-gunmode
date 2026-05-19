# ArenaGunGame – Configuration Reference

## File Location

```
oxide/config/ArenaGunGame.json
```

**Auto-generated** on first plugin load with defaults. Edit manually and reload plugin to apply changes.

---

## Config Structure

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
      "wear": ["metal.facemask", "metal.plate.torso", "roadsign.kilt", "roadsign.gloves", "shoes.boots"],
      "extras": [
        {"shortname": "syringe.medical", "amount": 3},
        {"shortname": "bandage", "amount": 5}
      ]
    },
    ...
  ]
}
```

---

## Global Settings

### `Cycle interval (seconds)`

**Type**: `float`  
**Default**: `240` (4 minutes)  
**Range**: `30–3600` (30 sec to 1 hour recommended)

**Purpose**: Time between weapon cycles. All online players receive new loadout simultaneously when timer fires.

**Examples**:
- `60` = 1-minute rounds (fast-paced, high cycling)
- `180` = 3-minute rounds (standard)
- `300` = 5-minute rounds (longer engagements)

**Reload**: Edit and run `/oxide.reload ArenaGunGame`.

---

## Loadout Structure

Each loadout in the `Loadouts` array defines one complete equipment set.

### Required Fields

#### `label` (string)
**Display name** shown in broadcast.

```json
"label": "AK-47"
```

Appears in: `[GunGame] Следующий weapon: <color>#AK-47</color>`

---

#### `weapon` (string)
**Weapon shortname** — exact Rust item name.

```json
"weapon": "rifle.ak"
```

**Common weapons**:
| Name | Shortname | Type |
|---|---|---|
| AK-47 | `rifle.ak` | Assault |
| LR-300 | `rifle.lr300` | Assault |
| M249 | `lmg.m249` | LMG |
| MP5 | `smg.mp5` | SMG |
| Thompson | `smg.thompson` | SMG |
| Pump Shotgun | `shotgun.pump` | Shotgun |
| SPAS-12 | `shotgun.spas12` | Shotgun |
| Bolt Action Sniper | `rifle.bolt` | Sniper |
| Semi-Auto Rifle | `rifle.semiauto` | Rifle |
| Python | `pistol.python` | Pistol |
| M92 Pistol | `pistol.m92` | Pistol |
| Crossbow | `crossbow` | Primitive |
| Compound Bow | `bow.compound` | Primitive |

**Full weapon list**: [Rust Item List](https://rustlabs.com/) (search for weapon shortnames).

---

#### `ammo` (string)
**Ammo shortname** — must match weapon's ammo type.

```json
"ammo": "ammo.rifle"
```

**Common ammo**:
| Type | Shortname | Used By |
|---|---|---|
| 5.56 Rifle | `ammo.rifle` | AK, LR-300, bolt, etc. |
| 5.56 HV | `ammo.rifle.hv` | Sniper rifles (optional higher tier) |
| 5.56 Semi | `ammo.rifle.semiauto` | Semi-Auto Rifle |
| Pistol | `ammo.pistol` | MP5, Thompson, M92, Python |
| Shotgun Buckshot | `ammo.shotgun` | SPAS-12, Pump Shotgun |
| Shotgun Slug | `ammo.shotgun.slug` | Slug rounds (higher damage) |
| Wooden Arrow | `arrow.wooden` | Crossbow, Bow |
| HV Arrow | `arrow.hv` | Compound Bow |

**Mismatch handling**: If ammo shortname is wrong, `ItemManager.CreateByName()` returns `null`; player gets weapon but no ammo. Check server log for errors.

---

#### `ammo_count` (integer)
**Quantity of ammo** to distribute.

```json
"ammo_count": 120
```

**Balancing**:
- **High damage weapons** (AK, sniper): 100–150 ammo
- **SMG** (MP5, Thompson): 150–250 ammo (spray)
- **Shotgun**: 30–64 rounds
- **Primitive** (bow, crossbow): 20–40

---

### Optional Fields

#### `attachments` (array of strings)
**Weapon modifications** (scopes, silencers, etc.).

```json
"attachments": ["weapon.mod.holosight", "weapon.mod.silencer"]
```

Each mod is created and attached to the weapon's internal inventory (`weapon.contents`).

**Common attachments**:
| Mod | Shortname | Effect |
|---|---|---|
| Holosight | `weapon.mod.holosight` | Reticle, no zoom |
| Laser Sight | `weapon.mod.lasersight` | Red dot, aim assist |
| Silencer | `weapon.mod.silencer` | Sound reduction, range reduction |
| Muzzle Brake | `weapon.mod.muzzlebrake` | Recoil reduction |
| Muzzle Boost | `weapon.mod.muzzleboost` | Damage/velocity increase |
| 4x Scope | `weapon.mod.small.scope` | 4x magnification |
| 8x Scope | `weapon.mod.8x.scope` | 8x magnification |
| 16x Scope | `weapon.mod.16x.scope` | 16x magnification |
| Flashlight | `weapon.mod.flashlight` | Illumination |

**Failsafe**: If attachment shortname is invalid, it's silently skipped (logged in console). Weapon still equips without the mod.

**Empty list**: Omit or use `[]` for no attachments.

```json
"attachments": []
```

---

#### `wear` (array of strings)
**Clothing & armor** equipped to player.

```json
"wear": ["metal.facemask", "metal.plate.torso", "roadsign.kilt", "roadsign.gloves", "shoes.boots"]
```

Each item goes into `containerWear` and auto-equips to the appropriate slot (head, chest, legs, hands, feet).

**Common clothing**:
| Item | Shortname | Slot | Tier |
|---|---|---|---|
| Metal Facemask | `metal.facemask` | Head | Heavy |
| Metal Chest Plate | `metal.plate.torso` | Chest | Heavy |
| Roadsign Kilt | `roadsign.kilt` | Legs | Medium |
| Roadsign Gloves | `roadsign.gloves` | Hands | Medium |
| Coffeecan Helmet | `coffeecan.helmet` | Head | Light |
| Riot Helmet | `riot.helmet` | Head | Medium |
| Hoodie | `hoodie` | Chest | Light |
| Jacket | `jacket` | Chest | Light |
| Pants | `pants` | Legs | Light |
| T-Shirt | `tshirt` | Chest | Light |
| Burlap Gloves | `burlap.gloves` | Hands | Light |
| Shoes/Boots | `shoes.boots` | Feet | Any |

**Loadout Progression** (common pattern):
- **Early rounds**: Light armor (hoodie, pants, coffeecan)
- **Mid rounds**: Medium armor (roadsign kilt/gloves)
- **Late rounds**: Heavy armor (metal chest/facemask)
- **Last rounds**: Minimal (t-shirt, pants) or no armor

**Slot Occupancy**: Only one item per slot equips. If two head items are listed, the second fails silently and moves to `containerMain`.

---

#### `extras` (array of objects)
**Medical, food, and utility items**.

```json
"extras": [
  {"shortname": "syringe.medical", "amount": 3},
  {"shortname": "bandage", "amount": 5},
  {"shortname": "can.tuna", "amount": 1}
]
```

All extras go into `containerMain` (backpack).

**Medical**:
| Item | Shortname | Effect | Amount |
|---|---|---|---|
| Medical Syringe | `syringe.medical` | +15 HP instant, +20 over time | 1–3 per round |
| Large Medkit | `largemedkit` | +100 HP | 0–1 per round |
| Bandage | `bandage` | +5 HP per use | 2–5 per round |
| Anti-Rad Pills | `antiradpills` | Radiation resistance | 0–1 |

**Food/Water**:
| Item | Shortname | Type |
|---|---|---|
| Tuna Can | `can.tuna` | Food |
| Bean Can | `can.beans` | Food |
| Apple | `apple` | Food |
| Water Jug | `water.jug` | Drink |

**Typical loadout**:
- Heavy weapons: 2–3 medical syringes, 3–5 bandages
- SMG/Light: 1 medical, 3–5 bandages
- Primitive (bow): 1–2 bandages only

---

## How to Modify

### Add a New Loadout

1. **Open** `oxide/config/ArenaGunGame.json`
2. **Find** the `"Loadouts"` array
3. **Copy** an existing loadout block
4. **Paste** as a new entry
5. **Edit** the fields:
   - Change `label` (display name)
   - Change `weapon` shortname
   - Adjust `ammo_count`
   - Add/remove `attachments`
   - Update `wear` for new armor style
   - Adjust `extras`

**Example** (adding Hunting Bow as a new round):

```json
{
  "label": "Hunting Bow",
  "weapon": "bow.hunting",
  "attachments": [],
  "ammo": "arrow.wooden",
  "ammo_count": 40,
  "wear": ["tshirt", "pants", "shoes.boots"],
  "extras": [
    {"shortname": "bandage", "amount": 3}
  ]
}
```

6. **Reload** plugin:
   ```
   /oxide.reload ArenaGunGame
   ```

---

### Remove a Loadout

1. **Open** config
2. **Delete** the entire loadout block (including trailing comma if needed)
3. **Reload** plugin

**Note**: Plugin cycles through remaining loadouts. Total count determines wraparound.

---

### Adjust Cycle Interval

1. **Open** config
2. **Find** `"Cycle interval (seconds)"`
3. **Change** value:
   ```json
   "Cycle interval (seconds)": 60
   ```
4. **Reload** plugin

---

### Rebalance Ammo Counts

**Too much ammo?** Reduce counts:
```json
"ammo_count": 100  // was 200
```

**Too little?** Increase:
```json
"ammo_count": 300  // was 150
```

**Balancing rule**: Early/powerful weapons get less ammo; spray weapons (SMG) get more.

---

### Change Loadout Progression

**Swap weapon order** by reordering loadout blocks in the array. The plugin cycles top-to-bottom.

Example: Move "Crossbow" to be first (hardest):
```json
"Loadouts": [
  { "label": "Crossbow", "weapon": "crossbow", ... },
  { "label": "M249", "weapon": "lmg.m249", ... },
  ...
]
```

Now cycle starts with Crossbow.

---

## Validation & Errors

### Syntax Errors

**Issue**: Invalid JSON (missing comma, quote, bracket).

**Fix**: Use a JSON validator (paste config into [jsonlint.com](https://www.jsonlint.com/)). Error message will point to line.

---

### Invalid Shortname

**Issue**: `"weapon": "rifle.unknown"` doesn't exist in Rust.

**Symptom**: Player joins, no weapon in inventory. Server log shows `ItemManager.CreateByName() returned null`.

**Fix**: Check shortname against [Rust item list](https://rustlabs.com/). Copy exact shortname.

---

### Attachment Not Attaching

**Issue**: `"attachments": ["weapon.mod.invalid"]` doesn't fit weapon.

**Symptom**: Weapon equips, but mod is missing. No error (silent graceful failure).

**Fix**: Verify attachment is compatible with weapon type (e.g., holosight on rifle, not grenade launcher).

---

### Wear Item Conflicting

**Issue**: Two head items listed; second one doesn't equip.

**Symptom**: Only first helmet/head item is worn; second is in backpack.

**Fix**: Include only one item per slot type.

---

## Common Recipes

### "Balanced" Loadout (for mid-round)

```json
{
  "label": "Balanced Rifle",
  "weapon": "rifle.ak",
  "attachments": ["weapon.mod.holosight"],
  "ammo": "ammo.rifle",
  "ammo_count": 120,
  "wear": ["coffeecan.helmet", "jacket", "pants", "burlap.gloves", "shoes.boots"],
  "extras": [
    {"shortname": "syringe.medical", "amount": 1},
    {"shortname": "bandage", "amount": 3}
  ]
}
```

---

### "Heavy" Loadout (for late round)

```json
{
  "label": "Heavy Suppression",
  "weapon": "lmg.m249",
  "attachments": ["weapon.mod.holosight", "weapon.mod.lasersight"],
  "ammo": "ammo.rifle",
  "ammo_count": 250,
  "wear": ["metal.facemask", "metal.plate.torso", "roadsign.kilt", "roadsign.gloves", "shoes.boots"],
  "extras": [
    {"shortname": "syringe.medical", "amount": 3},
    {"shortname": "bandage", "amount": 5},
    {"shortname": "can.tuna", "amount": 1}
  ]
}
```

---

### "Sniper" Loadout (long range)

```json
{
  "label": "Sniper L96",
  "weapon": "rifle.l96",
  "attachments": ["weapon.mod.16x.scope"],
  "ammo": "ammo.rifle.hv",
  "ammo_count": 30,
  "wear": ["coffeecan.helmet", "hoodie", "pants", "burlap.gloves", "shoes.boots"],
  "extras": [
    {"shortname": "syringe.medical", "amount": 1},
    {"shortname": "bandage", "amount": 2}
  ]
}
```

---

## Performance Notes

- **Config load time**: Negligible (JSON parse, ~10ms on server init)
- **Per-cycle time**: O(n×m) where n=player count, m=items per loadout
  - 20 players × 10 items ≈ 200 operations / cycle → instant
- **No runtime cost** of unused fields

---

## Defaults (Built-In)

If config is missing or corrupted, plugin regenerates with this default set:

1. M249 – Heavy
2. AK-47 – Standard
3. LR-300 – Scoped
4. SPAS-12 – Close
5. Pump Shotgun – Slug
6. MP5 – SMG Silent
7. Thompson – SMG Mid
8. Semi-Auto Rifle – Mid Range
9. Python – Revolver
10. Bolt Action – Sniper
11. Crossbow – Primitive

Cycle interval: `240` seconds (4 minutes).

---

## Reload Command

After editing config:

```
/oxide.reload ArenaGunGame
```

**Effect**:
- Reloads config from disk
- Resets `_weaponIndex` to 0
- Restarts cycle timer with new interval

**No restart needed** — changes live within seconds.
