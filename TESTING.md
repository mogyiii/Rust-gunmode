# ArenaGunGame – Testing Documentation

## Test Objectives

Verify:
1. Global weapon cycle distributes loadouts correctly
2. Round stats track kills/deaths accurately
3. Environmental sandbox (no resource gather, no loot)
4. Player spawn/respawn behavior
5. Configuration parsing
6. Edge cases (disconnects, suicides, self-damage)

---

## Unit Test Scenarios

### T1 – Plugin Initialization

**Setup**: Start server with plugin loaded.

**Expected**:
- `_weaponIndex` = 0
- `_cycleTimer` active (Repeat every CycleInterval)
- First loadout ready in memory

**Check**: Console should have no errors. Run `/plugins` or Oxide load check.

---

### T2 – Player Connect

**Setup**: Player joins arena.

**Expected**:
- Player receives loadout matching current `_weaponIndex`
- Inventory contains: weapon (belt) + ammo (main) + clothing (wear) + healing (main)
- No errors in server log

**How to Test**:
```
1. Connect player
2. Check inventory: should have weapon in belt, ammo in backpack, armor equipped
3. Open console: should see equipment appear instantly
```

---

### T3 – Weapon Cycle Broadcast

**Setup**: Wait for cycle timer to fire (default 4 min).

**Expected**:
- Previous round: broadcast "Legtöbb kill: [player] — XK / YD"
- If different player: separate "Legjobb KDA: [player] — XK / YD (Z.ZZ)"
- New weapon broadcast: "[GunGame] Következő fegyver: <color>[name]</color>"
- All players receive new loadout **simultaneously**

**How to Test**:
```
1. Multiple players kill each other for 4 min
2. At 4:00, check chat for round results
3. Open inventory: all players should have new weapon + ammo
```

---

### T4 – Kill Tracking

**Scenario A: Normal PvP Kill**

**Setup**: Player A shoots Player B.

**Expected**:
- `_roundStats[A.userID].Kills++`
- `_roundStats[B.userID].Deaths++`
- Both visible at round end broadcast

**How to Test**: Player A kills Player B, both die at round end, check broadcast.

---

**Scenario B: Self-Kill (Jump into Void)**

**Setup**: Player suicides (no attacker).

**Expected**:
- `_roundStats[player.userID].Deaths++`
- No kill credited to anyone

**How to Test**: Player jumps off cliff or drowns. At round end, their Deaths increase, no one's Kills increase.

---

**Scenario C: Self-Shot (Gun Bounce / Rocket)**

**Setup**: Player A shoots rocket, bounces back, kills Player A.

**Expected**:
- Only `Deaths++` for A
- Check: `attacker.userID != victim.userID` prevents self-kill from counting as kill

**How to Test**: Rare, but ensure suicide-by-reflected-damage doesn't credit a kill.

---

**Scenario D: Disconnect During Round**

**Setup**: 
- Player A kills 5 people
- Player A disconnects
- Round ends before reconnect

**Expected**:
- `_roundStats[A.userID]` still exists (keyed by SteamID, not name)
- Broadcast shows "Player A — 5K / 0D"
- Stats clear on next cycle

**How to Test**: Have player rack up kills, disconnect, observe round announcement.

---

### T5 – Environmental Sandbox

**Scenario A: Tree Damage**

**Setup**: Player shoots tree with rifle.

**Expected**:
- Tree does **not** take damage
- Tree does **not** drop wood
- No hit particles

**How to Test**:
```
1. Equip rifle, aim at tree
2. Fire multiple shots
3. Tree should remain intact
```

**Verify in Code**: `OnEntityTakeDamage` checks `entity is TreeEntity`, calls `info.damageTypes.ScaleAll(0f)`.

---

**Scenario B: Ore / Rock Damage**

**Setup**: Player shoots ore/rock with pickaxe or explosives.

**Expected**:
- Rock invulnerable
- No ore drop

**How to Test**: Try to destroy stone outcrops with explosives; they don't break.

---

**Scenario C: Barrel / Crate Loot Blocked**

**Setup**: Player walks to barrel/crate, presses use key.

**Expected**:
- UI does **not** open
- `CanLootEntity` returns `false`

**How to Test**: Approach a barrel or crate, try to open—nothing happens.

---

**Scenario D: Corpse Loot Blocked**

**Setup**: Player A kills Player B.

**Expected**:
- Player B's corpse appears
- Other players try to loot corpse
- UI does **not** open
- `CanLootEntity` returns `false` for `LootableCorpse`

**How to Test**:
```
1. Kill a player
2. Approach corpse
3. Press use/loot key
4. UI should not open
```

---

**Scenario E: Dropped Bag Blocked**

**Setup**: Player dies, drops a bag on ground.

**Expected**:
- `DroppedItemContainer` entity exists
- Players cannot loot it
- `CanLootEntity` returns `false`

**How to Test**: Die with items, drop bag appears; try to loot—denied.

---

### T6 – Player-on-Player Looting

**Setup**: Player A dies, Player B tries to loot corpse (if available).

**Expected**:
- `CanLootPlayer` returns `false`
- No inventory transfer

**How to Test**: (Depends on Rust looting mechanics; may be superseded by `CanLootEntity`.)

---

### T7 – Respawn with Loadout

**Setup**: Player dies during round.

**Expected**:
- `OnPlayerRespawned` fires
- `GiveGlobalWeapon` called
- Player respawns with **current** weapon (not previous), empty inventory

**How to Test**:
```
1. Die multiple times in same round
2. Verify each respawn gives same weapon (cycle hasn't fired yet)
3. On next cycle, verify new weapon granted
```

---

### T8 – Configuration Parsing

**Setup**: Modify `oxide/config/ArenaGunGame.json`:
- Change `CycleInterval` to 60 (1 min)
- Add/remove a loadout
- Edit ammo counts

**Expected**:
- Plugin reloads config on `/oxide.reload ArenaGunGame`
- Timer respects new interval
- New loadout rotates in

**How to Test**:
```
1. Stop server
2. Edit config JSON
3. Reload plugin: /oxide.reload ArenaGunGame
4. Verify changes take effect
```

---

### T9 – Invalid Item Shortnames

**Setup**: Config has typo: `"weapon": "rifle.ak_typo"`

**Expected**:
- `ItemManager.CreateByName("rifle.ak_typo", 1)` returns `null`
- Early return in `GiveGlobalWeapon`; player keeps empty inventory
- Server log shows no error (silent graceful failure)

**How to Test**:
```
1. Intentionally misname a weapon in config
2. Reload plugin
3. Player joins → no weapon
4. Verify no crash; observe log for null item
```

**Fix**: Correct the shortname in config.

---

### T10 – KDA Calculation (Edge Case)

**Setup**: 
- Player A: 10 kills, 0 deaths
- Player B: 5 kills, 5 deaths

**Expected**:
- A's KDA = 10 / max(0, 1) = 10.00
- B's KDA = 5 / 5 = 1.00
- A appears as best KDA

**Formula**: `KDA = Kills / (Deaths > 0 ? Deaths : 1)`

**How to Test**: Farm kills without dying (A) vs. balanced player (B). Observe broadcast.

---

## Integration Test Scenarios

### IT1 – Full 3-Cycle Round-Robin

**Setup**: 3-player server, CycleInterval = 60 seconds.

**Steps**:
1. Players join → all get Loadout[0]
2. Fight for 60 sec, track kills
3. Cycle fires → broadcast stats, new weapon (Loadout[1])
4. Fight for 60 sec
5. Cycle fires → broadcast, new weapon (Loadout[2])
6. Fight for 60 sec
7. Cycle fires → broadcast, wrap back to Loadout[0]

**Expected**:
- Stats announced each time
- Weapon always changes
- Cycle count matches number of broadcasts
- Kill leaders rotate

---

### IT2 – Connection/Disconnection Mid-Round

**Setup**: 4 players, mid-game.

**Steps**:
1. Players fight, accumulate kills (A: 3 kills, B: 2 kills, C: 1 kill, D: 0 kills)
2. Player B disconnects
3. Cycle fires
4. Broadcast shows B's stats (2 kills)
5. Player B reconnects, gets new loadout

**Expected**:
- B's 2 kills appear in broadcast even though disconnected
- B rejoins with correct new loadout

---

### IT3 – Loadout Integrity (All Items Present)

**Setup**: Join server with complex loadout (e.g., M249: weapon + 2 attachments + ammo + 5 clothing + 2 healing).

**Expected**:
- Belt: M249 weapon with scopes attached
- Main inventory: 200x ammo.rifle, 3x syringe.medical, 5x bandage
- Wear: All 5 clothing items equipped (helmet, chest, legs, hands, feet)
- Total items: 1 weapon (belt) + ~10 items (main) + 5 clothing (wear) = ~16 items

**How to Test**: Open inventory, verify all items present and correct counts.

---

### IT4 – Rapid Respawns

**Setup**: Player dies and respawns repeatedly.

**Expected**:
- Each respawn grants current loadout
- No inventory drift
- Stats accumulate correctly (each death counted)

**How to Test**: Fall off cliff repeatedly. Each time should get weapon/ammo/healing.

---

## Stress / Edge Case Tests

### ST1 – Many Players (20+)

**Setup**: Stress test: 20 players on server.

**Expected**:
- Cycle fires without lag
- `GiveGlobalWeapon` runs for all 20 simultaneously
- No timeout or stack overflow
- Stats broadcast completes

**Note**: Performance-dependent; O(n) where n = player count + loadout items.

---

### ST2 – Long Uptime (8 hour run)

**Expected**:
- Timer doesn't drift
- Memory stable (no leaks in `_roundStats` clearing)
- Plugin remains responsive

---

### ST3 – Loadout with Rare/Missing Items

**Setup**: Config references item that doesn't exist in Rust (e.g., typo in attachment).

**Expected**:
- `ItemManager.CreateByName("weapon.mod.invalid", 1)` returns `null`
- Graceful skip: `if (mod != null) ...`
- Player still gets weapon + ammo (attachment just skipped)

**How to Test**: Add typo to attachment name, join server.

---

## Manual Acceptance Tests

### MA1 – Gameplay Feel

**Criteria**:
- [ ] Weapon changes feel fair (global, simultaneous)
- [ ] Loadouts are balanced (no one weapon dominates)
- [ ] Healing items sufficient for round duration
- [ ] Round announcements are readable/clear

**How**: Play a few rounds as intended, gather feedback.

---

### MA2 – Admin/Config Experience

**Criteria**:
- [ ] Config JSON is well-formatted, easy to edit
- [ ] Adding a new loadout is straightforward
- [ ] Reload command works smoothly
- [ ] No need to restart server for config changes

**How**: Modify config, reload, verify changes.

---

## Test Checklist

- [ ] T1 – Initialization
- [ ] T2 – Player Connect
- [ ] T3 – Broadcast
- [ ] T4 – Kill Tracking (A, B, C, D)
- [ ] T5 – Environmental Sandbox (A, B, C, D, E)
- [ ] T6 – Player Looting
- [ ] T7 – Respawn
- [ ] T8 – Config
- [ ] T9 – Invalid Shortnames
- [ ] T10 – KDA Math
- [ ] IT1 – Full Round-Robin
- [ ] IT2 – Connect/Disconnect
- [ ] IT3 – Loadout Integrity
- [ ] IT4 – Rapid Respawns
- [ ] ST1 – Many Players
- [ ] ST2 – Long Uptime
- [ ] ST3 – Missing Items
- [ ] MA1 – Gameplay Feel
- [ ] MA2 – Config Experience
