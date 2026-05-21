/*
 * ArenaGunGame – Carbon/Oxide  |  Zero dependencies
 *
 * HOOKS USED
 * ──────────────────────────────────────────────────────────────────────
 * OnServerInitialized          – starts the global cycle timer
 * OnPlayerRespawned            – gives current loadout after respawn
 * OnPlayerConnected            – gives current loadout on first join
 * OnPlayerDeath                – tracks kills/deaths for round stats
 * OnEntityTakeDamage           – sets all damage to 0 on world entities
 *                                (TreeEntity, OreResourceEntity, LootContainer)
 * OnDispenserGather            – blocks resource yield from trees/rocks
 * OnDispenserBonus             – blocks bonus resource yield
 * CanPickupItem                – blocks picking up items from the ground
 * CanLootEntity                – blocks opening corpses, bags, containers
 * CanLootPlayer                – blocks player-on-player looting
 *
 * MILESTONE 3 CHANGES
 * ──────────────────────────────────────────────────────────────────────
 * • All loadouts: binoculars (belt)
 * • Heavy armor (metal.facemask): nightvision.goggles in main inventory
 * • Medium armor (coffeecan): torch in belt
 * • Light armor (hoodie/tshirt): torch or flashlight.held in belt
 * • All loadouts: can.beans added, bandage 5→10, more syringes
 * • 7 new loadouts: L96, M39 Rifle, Custom SMG, Revolver,
 *                   Semi-Auto Pistol, Compound Bow, Double Barrel
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using Carbon.Base;
using Newtonsoft.Json;
using Oxide.Core.Libraries;
using UnityEngine;

namespace Carbon.Plugins
{
    [Info("ArenaGunGame", "Mogy", "1.1.0")]
    [Description("Standalone Arena GunGame – zero dependencies")]
    public class ArenaGunGame : CarbonPlugin
    {
        private enum GameMode { GunGame, KingOfTheHill }

        // ── Configuration ──────────────────────────────────────────────
        private Configuration _cfg;

        private class Configuration
        {
            [JsonProperty("Cycle interval (seconds)")]
            public float CycleInterval = 240f;

            [JsonProperty("Game mode (GunGame / KingOfTheHill)")]
            public string Mode = "GunGame";

            [JsonProperty("KoTH: zone radius (meters)")]
            public float KothRadius = 150f;

            [JsonProperty("KoTH: hill rotate interval (seconds)")]
            public float KothRotateInterval = 180f;

            [JsonProperty("KoTH: zone points per second (in zone)")]
            public int KothZonePointsPerSec = 10;

            [JsonProperty("KoTH: points per kill")]
            public int KothKillPoints = 100;

            [JsonProperty("KoTH: tick interval (seconds)")]
            public float KothRadInterval = 1f;

            [JsonProperty("Loadouts")]
            public List<Loadout> Loadouts = new List<Loadout>
            {
                // 1 ── M249 – heavy suppression  [heavy armor → NVG in main]
                new Loadout
                {
                    Label       = "M249",
                    Weapon      = "lmg.m249",
                    Attachments = new List<string> { "weapon.mod.holosight", "weapon.mod.lasersight" },
                    Ammo        = "ammo.rifle",
                    AmmoCount   = 2000,
                    Wear        = new List<string>
                    {
                        "metal.facemask", "metal.plate.torso", "roadsign.kilt",
                        "roadsign.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",        1,  "belt"),
                        new ItemStack("binoculars",          1,  "belt"),
                        new ItemStack("nightvision.goggles", 1,  "main"),
                        new ItemStack("grenade.f1",          2,  "belt"),
                        new ItemStack("syringe.medical",     4,  "main"),
                        new ItemStack("bandage",             10, "main"),
                        new ItemStack("can.tuna",            3,  "main"),
                        new ItemStack("can.beans",           2,  "main"),
                        new ItemStack("lowgradefuel",        30, "main"),
                    }
                },

                // 2 ── AK-47 – standard assault  [heavy armor → NVG in main]
                new Loadout
                {
                    Label       = "AK-47",
                    Weapon      = "rifle.ak",
                    Attachments = new List<string> { "weapon.mod.holosight", "weapon.mod.muzzlebrake" },
                    Ammo        = "ammo.rifle",
                    AmmoCount   = 1200,
                    Wear        = new List<string>
                    {
                        "metal.facemask", "metal.plate.torso", "roadsign.kilt",
                        "roadsign.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",        1,  "belt"),
                        new ItemStack("binoculars",          1,  "belt"),
                        new ItemStack("nightvision.goggles", 1,  "main"),
                        new ItemStack("grenade.f1",          2,  "belt"),
                        new ItemStack("syringe.medical",     3,  "main"),
                        new ItemStack("bandage",             10, "main"),
                        new ItemStack("can.tuna",            3,  "main"),
                        new ItemStack("can.beans",           2,  "main"),
                        new ItemStack("lowgradefuel",        30, "main"),
                    }
                },

                // 3 ── LR-300 – scoped assault  [heavy armor → NVG in main]
                new Loadout
                {
                    Label       = "LR-300",
                    Weapon      = "rifle.lr300",
                    Attachments = new List<string> { "weapon.mod.small.scope", "weapon.mod.muzzlebrake" },
                    Ammo        = "ammo.rifle",
                    AmmoCount   = 1200,
                    Wear        = new List<string>
                    {
                        "metal.facemask", "metal.plate.torso", "roadsign.kilt",
                        "roadsign.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",        1,  "belt"),
                        new ItemStack("binoculars",          1,  "belt"),
                        new ItemStack("nightvision.goggles", 1,  "main"),
                        new ItemStack("grenade.f1",          2,  "belt"),
                        new ItemStack("syringe.medical",     3,  "main"),
                        new ItemStack("bandage",             10, "main"),
                        new ItemStack("can.tuna",            3,  "main"),
                        new ItemStack("can.beans",           2,  "main"),
                        new ItemStack("lowgradefuel",        30, "main"),
                    }
                },

                // 4 ── SPAS-12 – close combat  [medium armor → torch]
                new Loadout
                {
                    Label       = "SPAS-12",
                    Weapon      = "shotgun.spas12",
                    Attachments = new List<string> { "weapon.mod.lasersight", "weapon.mod.flashlight" },
                    Ammo        = "ammo.shotgun",
                    AmmoCount   = 640,
                    Wear        = new List<string>
                    {
                        "coffeecan.helmet", "metal.plate.torso", "roadsign.kilt",
                        "roadsign.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 3,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 5 ── Pump Shotgun – slug  [medium armor → torch]
                new Loadout
                {
                    Label       = "Pump Shotgun",
                    Weapon      = "shotgun.pump",
                    Attachments = new List<string> { "weapon.mod.lasersight" },
                    Ammo        = "ammo.shotgun.slug",
                    AmmoCount   = 320,
                    Wear        = new List<string>
                    {
                        "coffeecan.helmet", "jacket", "pants",
                        "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 3,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 6 ── MP5 – silenced SMG  [medium armor → torch]
                new Loadout
                {
                    Label       = "MP5",
                    Weapon      = "smg.mp5",
                    Attachments = new List<string> { "weapon.mod.silencer", "weapon.mod.holosight" },
                    Ammo        = "ammo.pistol",
                    AmmoCount   = 2000,
                    Wear        = new List<string>
                    {
                        "coffeecan.helmet", "hoodie", "pants",
                        "roadsign.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 7 ── Thompson – mid SMG  [light armor → torch]
                new Loadout
                {
                    Label       = "Thompson",
                    Weapon      = "smg.thompson",
                    Attachments = new List<string> { "weapon.mod.muzzleboost", "weapon.mod.lasersight" },
                    Ammo        = "ammo.pistol",
                    AmmoCount   = 1800,
                    Wear        = new List<string>
                    {
                        "hoodie", "pants", "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 8 ── Semi-Auto Rifle – mid range  [light armor → torch]
                new Loadout
                {
                    Label       = "Semi-Auto Rifle",
                    Weapon      = "rifle.semiauto",
                    Attachments = new List<string> { "weapon.mod.small.scope", "weapon.mod.silencer" },
                    Ammo        = "ammo.rifle.semiauto",
                    AmmoCount   = 1000,
                    Wear        = new List<string>
                    {
                        "hoodie", "pants", "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 9 ── Python – revolver  [light armor → flashlight]
                new Loadout
                {
                    Label       = "Python",
                    Weapon      = "pistol.python",
                    Attachments = new List<string> { "weapon.mod.lasersight" },
                    Ammo        = "ammo.pistol",
                    AmmoCount   = 480,
                    Wear        = new List<string>
                    {
                        "tshirt", "pants", "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("flashlight.held", 1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 10 ── Bolt Action – sniper  [medium armor → torch]
                new Loadout
                {
                    Label       = "Bolt Action Sniper",
                    Weapon      = "rifle.bolt",
                    Attachments = new List<string> { "weapon.mod.8x.scope" },
                    Ammo        = "ammo.rifle.hv",
                    AmmoCount   = 300,
                    Wear        = new List<string>
                    {
                        "coffeecan.helmet", "jacket", "pants",
                        "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 11 ── Crossbow – primitive  [light armor → torch]
                new Loadout
                {
                    Label       = "Crossbow",
                    Weapon      = "crossbow",
                    Attachments = new List<string>(),
                    Ammo        = "arrow.wooden",
                    AmmoCount   = 300,
                    Wear        = new List<string>
                    {
                        "tshirt", "pants", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 12 ── L96 – elite sniper  [medium armor → torch]
                new Loadout
                {
                    Label       = "L96 Sniper",
                    Weapon      = "rifle.l96",
                    Attachments = new List<string> { "weapon.mod.8x.scope" },
                    Ammo        = "ammo.rifle.hv",
                    AmmoCount   = 200,
                    Wear        = new List<string>
                    {
                        "metal.facemask", "jacket", "pants",
                        "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",        1,  "belt"),
                        new ItemStack("binoculars",          1,  "belt"),
                        new ItemStack("nightvision.goggles", 1,  "main"),
                        new ItemStack("grenade.f1",          2,  "belt"),
                        new ItemStack("syringe.medical",     2,  "main"),
                        new ItemStack("bandage",             10, "main"),
                        new ItemStack("can.tuna",            3,  "main"),
                        new ItemStack("can.beans",           2,  "main"),
                        new ItemStack("lowgradefuel",        30, "main"),
                    }
                },

                // 13 ── M39 – DMR  [medium armor → torch]
                new Loadout
                {
                    Label       = "M39 Rifle",
                    Weapon      = "rifle.m39",
                    Attachments = new List<string> { "weapon.mod.small.scope", "weapon.mod.muzzlebrake" },
                    Ammo        = "ammo.rifle.semiauto",
                    AmmoCount   = 800,
                    Wear        = new List<string>
                    {
                        "coffeecan.helmet", "jacket", "pants",
                        "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 14 ── Custom SMG – silenced  [light armor → torch]
                new Loadout
                {
                    Label       = "Custom SMG",
                    Weapon      = "smg.2",
                    Attachments = new List<string> { "weapon.mod.silencer", "weapon.mod.lasersight" },
                    Ammo        = "ammo.pistol",
                    AmmoCount   = 1800,
                    Wear        = new List<string>
                    {
                        "hoodie", "pants", "burlap.gloves", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 15 ── Revolver  [light armor → flashlight]
                new Loadout
                {
                    Label       = "Revolver",
                    Weapon      = "pistol.revolver",
                    Attachments = new List<string> { "weapon.mod.lasersight" },
                    Ammo        = "ammo.pistol",
                    AmmoCount   = 360,
                    Wear        = new List<string>
                    {
                        "tshirt", "pants", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("flashlight.held", 1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 16 ── Semi-Auto Pistol  [light armor → flashlight]
                new Loadout
                {
                    Label       = "Semi-Auto Pistol",
                    Weapon      = "pistol.semiauto",
                    Attachments = new List<string> { "weapon.mod.silencer", "weapon.mod.lasersight" },
                    Ammo        = "ammo.pistol",
                    AmmoCount   = 600,
                    Wear        = new List<string>
                    {
                        "tshirt", "pants", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("flashlight.held", 1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 17 ── Compound Bow  [light armor → torch]
                new Loadout
                {
                    Label       = "Compound Bow",
                    Weapon      = "bow.compound",
                    Attachments = new List<string>(),
                    Ammo        = "arrow.hv",
                    AmmoCount   = 200,
                    Wear        = new List<string>
                    {
                        "tshirt", "pants", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("torch",           1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 2,  "main"),
                        new ItemStack("bandage",         10, "main"),
                        new ItemStack("can.tuna",        3,  "main"),
                        new ItemStack("can.beans",       2,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },

                // 18 ── Double Barrel – extreme CQC  [light armor → flashlight]
                new Loadout
                {
                    Label       = "Double Barrel",
                    Weapon      = "shotgun.double",
                    Attachments = new List<string> { "weapon.mod.lasersight" },
                    Ammo        = "ammo.shotgun",
                    AmmoCount   = 200,
                    Wear        = new List<string>
                    {
                        "tshirt", "pants", "shoes.boots"
                    },
                    Extras = new List<ItemStack>
                    {
                        new ItemStack("knife.combat",    1,  "belt"),
                        new ItemStack("binoculars",      1,  "belt"),
                        new ItemStack("flashlight.held", 1,  "belt"),
                        new ItemStack("grenade.beancan", 2,  "belt"),
                        new ItemStack("syringe.medical", 3,  "main"),
                        new ItemStack("bandage",         15, "main"),
                        new ItemStack("can.tuna",        4,  "main"),
                        new ItemStack("can.beans",       3,  "main"),
                        new ItemStack("lowgradefuel",    30, "main"),
                    }
                },
            };
        }

        private class Loadout
        {
            [JsonProperty("label")]        public string         Label       = "";
            [JsonProperty("weapon")]       public string         Weapon;
            [JsonProperty("attachments")]  public List<string>   Attachments = new List<string>();
            [JsonProperty("ammo")]         public string         Ammo;
            [JsonProperty("ammo_count")]   public int            AmmoCount;
            [JsonProperty("wear")]         public List<string>   Wear        = new List<string>();
            [JsonProperty("extras")]       public List<ItemStack> Extras      = new List<ItemStack>();
        }

        private class ItemStack
        {
            [JsonProperty("shortname")]   public string Shortname;
            [JsonProperty("amount")]      public int    Amount;
            [JsonProperty("container")]   public string Container;

            public ItemStack(string sn, int amt, string container = "main")
            {
                Shortname = sn;
                Amount    = amt;
                Container = container;
            }
        }

        private class RoundStats
        {
            public ulong  SteamId;
            public string Name;
            public int    Kills;
            public int    Deaths;
            public int    KothScore;
            public float  KDA => (float)Kills / (Deaths > 0 ? Deaths : 1);
        }

        protected override void LoadDefaultConfig() { _cfg = new Configuration(); SaveConfig(); }
        protected override void LoadConfig()         { base.LoadConfig(); _cfg = Config.ReadObject<Configuration>(); }
        protected override void SaveConfig()         => Config.WriteObject(_cfg, true);

        // ── Global state ───────────────────────────────────────────────
        private int      _weaponIndex;
        private GameMode _activeMode;
        private readonly Dictionary<ulong, RoundStats> _roundStats  = new Dictionary<ulong, RoundStats>();
        private readonly List<Vector3>                 _spawnPoints  = new List<Vector3>();

        // KoTH state
        private Vector3                _hillPosition;
        private MapMarkerGenericRadius _hillMarker;
        private Action                 _kothRotateTimer;
        private Action                 _kothTickTimer;
        private readonly HashSet<ulong> _inZonePlayers = new HashSet<ulong>();


        // ── Lifecycle ──────────────────────────────────────────────────
        private void OnServerInitialized()
        {
            _weaponIndex = 0;
            System.Enum.TryParse(_cfg.Mode, ignoreCase: true, result: out _activeMode);
            GenerateSpawnPoints(60);
            timer.Repeat(_cfg.CycleInterval, 0, CycleWeapons);

            if (_activeMode == GameMode.KingOfTheHill)
                StartKingOfTheHill();
        }

        private void Unload()
        {
            _kothRotateTimer?.Invoke();
            _kothTickTimer?.Invoke();
            if (_hillMarker != null && !_hillMarker.IsDestroyed)
                _hillMarker.Kill();
        }

        // ── Spawn ──────────────────────────────────────────────────────
        private void GenerateSpawnPoints(int count)
        {
            _spawnPoints.Clear();

            float half     = World.Size * 0.35f;
            int   attempts = 0;

            while (_spawnPoints.Count < count && attempts < count * 15)
            {
                attempts++;
                float x = Random.Range(-half, half);
                float z = Random.Range(-half, half);
                // GetHeight(Vector3) uses world-space position
                float y = TerrainMeta.HeightMap.GetHeight(new Vector3(x, 0f, z));

                if (y > 0.5f)
                    _spawnPoints.Add(new Vector3(x, y + 0.5f, z));
            }

            Puts($"[GunGame] Generated {_spawnPoints.Count} spawn points (World.Size={World.Size}).");
        }

        private Vector3 GetRandomSpawnPoint()
        {
            if (_spawnPoints.Count == 0) return Vector3.zero;
            return _spawnPoints[Random.Range(0, _spawnPoints.Count)];
        }

        private Vector3 GetRandomMapPosition()
        {
            float half = World.Size * 0.35f;
            for (int i = 0; i < 100; i++)
            {
                float x = Random.Range(-half, half);
                float z = Random.Range(-half, half);
                float y = TerrainMeta.HeightMap.GetHeight(new Vector3(x, 0f, z));
                if (y > 0.5f) return new Vector3(x, y + 1f, z);
            }
            // Fallback: random position near centre above sea level
            return new Vector3(Random.Range(-50f, 50f), 10f, Random.Range(-50f, 50f));
        }

        // ── King of the Hill ───────────────────────────────────────────
        private void StartKingOfTheHill()
        {
            _hillPosition = GetRandomMapPosition();
            UpdateHillMarker();
            AnnounceHillPosition();

            _kothRotateTimer?.Invoke();
            var rotT = timer.Repeat(_cfg.KothRotateInterval, 0, RotateHill);
            _kothRotateTimer = () => rotT.Destroy();

            _kothTickTimer?.Invoke();
            var tickT = timer.Repeat(_cfg.KothRadInterval, 0, KothTick);
            _kothTickTimer = () => tickT.Destroy();
        }

        private void StopKingOfTheHill()
        {
            _kothRotateTimer?.Invoke();
            _kothTickTimer?.Invoke();
            if (_hillMarker != null && !_hillMarker.IsDestroyed)
                _hillMarker.Kill();
        }

        private void RotateHill()
        {
            _inZonePlayers.Clear();
            _hillPosition = GetRandomMapPosition();
            UpdateHillMarker();
            AnnounceHillPosition();
        }

        private void KothTick()
        {
            var nowInZone = new HashSet<ulong>();

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player == null || player.IsDead() || player.IsSleeping()) continue;

                bool inside = Vector3.Distance(player.transform.position, _hillPosition) <= _cfg.KothRadius;

                if (inside)
                {
                    nowInZone.Add(player.userID);

                    if (!_inZonePlayers.Contains(player.userID))
                        player.ChatMessage("<color=#ffcc00>[KoTH]</color> You entered the zone! +10 pts/sec");

                    var stats = GetOrCreateStats(player.userID, player.displayName);
                    stats.KothScore += _cfg.KothZonePointsPerSec;

                    if (stats.KothScore % 100 == 0)
                        player.ChatMessage($"<color=#ffcc00>[KoTH]</color> Zone points: <color=#aaffaa>{stats.KothScore}</color>");
                }
                else if (_inZonePlayers.Contains(player.userID))
                {
                    player.ChatMessage("<color=#ff6666>[KoTH]</color> You left the zone!");
                }
            }

            _inZonePlayers.Clear();
            foreach (var id in nowInZone) _inZonePlayers.Add(id);
        }

        private void UpdateHillMarker()
        {
            if (_hillMarker != null && !_hillMarker.IsDestroyed)
                _hillMarker.Kill();

            _hillMarker = GameManager.server.CreateEntity(
                "assets/prefabs/tools/map/genericradiusmarker.prefab",
                _hillPosition) as MapMarkerGenericRadius;

            if (_hillMarker == null) return;

            _hillMarker.alpha  = 0.5f;
            _hillMarker.color1 = new Color(1f, 0.8f, 0f);
            _hillMarker.color2 = new Color(1f, 0.4f, 0f);

            // ── MAP CIRCLE SIZE ───────────────────────────────────────────
            // The radius value below controls how big the orange circle looks
            // on the in-game map. It is NOT in meters — it is a raw display unit.
            //
            // Formula currently:  KothRadius * 150 / World.Size
            //   KothRadius = 150m (game-logic zone size, set in config)
            //   World.Size = server map size in meters (e.g. 1500)
            //   Result     = 15  (the actual value passed to the marker)
            //
            // HOW TO TUNE:
            //   If the circle is too LARGE  → lower the multiplier (150f → 75f, 50f, etc.)
            //   If the circle is too SMALL  → raise the multiplier (150f → 300f, 500f, etc.)
            //   You can also replace the whole expression with a plain number, e.g.:
            //       _hillMarker.radius = 8f;
            // ─────────────────────────────────────────────────────────────
            _hillMarker.radius = _cfg.KothRadius * 26f / World.Size;
            _hillMarker.Spawn();
            _hillMarker.SendUpdate();
        }

        private void AnnounceHillPosition()
        {
            var grid = GetGridReference(_hillPosition);
            Server.Broadcast(
                $"<color=#ffcc00>[KoTH]</color> New zone: <color=#ff6600>{grid}</color>" +
                $" ({_hillPosition.x:F0}, {_hillPosition.z:F0})" +
                $" — step inside to earn points! (Map: orange circle)");
        }

        private string GetGridReference(Vector3 pos)
        {
            float worldSize = World.Size;
            // ~150m cells — matches Rust's native grid for any map size
            int   cells    = Mathf.Max(1, Mathf.FloorToInt(worldSize / 150f));
            float cellSize = worldSize / cells;
            int col = Mathf.Clamp(Mathf.FloorToInt((pos.x + worldSize * 0.5f) / cellSize), 0, cells - 1);
            int row = Mathf.Clamp(Mathf.FloorToInt((worldSize * 0.5f - pos.z) / cellSize), 0, cells - 1);
            return $"{(char)('A' + col)}{row}";
        }

        // ── Weapon cycle ───────────────────────────────────────────────
        private void CycleWeapons()
        {
            AnnounceRoundStats();
            _roundStats.Clear();
            CleanupDroppedItems();

            _weaponIndex = (_weaponIndex + 1) % _cfg.Loadouts.Count;

            foreach (var player in BasePlayer.activePlayerList)
                GiveGlobalWeapon(player);

            var label = _cfg.Loadouts[_weaponIndex].Label;
            Server.Broadcast($"[GunGame] Next weapon: <color=#ffcc00>{label}</color>");
        }

        private void AnnounceRoundStats()
        {
            if (_roundStats.Count == 0 || _roundStats.Values.All(s => s.Kills == 0 && s.Deaths == 0)) return;

            if (_activeMode == GameMode.KingOfTheHill)
            {
                var kothTop = _roundStats.Values
                    .OrderByDescending(s => s.KothScore)
                    .Take(3)
                    .ToList();

                Server.Broadcast("<color=#ffcc00>[KoTH] ══ Round Over – Leaderboard ══</color>");
                for (int i = 0; i < kothTop.Count; i++)
                {
                    var s = kothTop[i];
                    Server.Broadcast(
                        $"<color=#ffcc00>[KoTH]</color> #{i + 1} <color=#ffff00>{s.Name}</color>" +
                        $" — <color=#aaffaa>{s.KothScore} pts</color>" +
                        $" ({s.Kills}K · {s.Deaths}D)");
                }

                foreach (var player in BasePlayer.activePlayerList)
                {
                    if (!_roundStats.TryGetValue(player.userID, out var ps)) continue;
                    var rank    = kothTop.FindIndex(s => s.SteamId == player.userID) + 1;
                    var rankStr = rank > 0 ? $" (#{rank})" : "";
                    player.ChatMessage(
                        $"<color=#aaffff>[KoTH] Your score{rankStr}:</color>" +
                        $" <color=#aaffaa>{ps.KothScore} pts</color> | {ps.Kills}K / {ps.Deaths}D");
                }
            }
            else
            {
                var top3 = _roundStats.Values
                    .OrderByDescending(s => s.Kills)
                    .ThenByDescending(s => s.KDA)
                    .Take(3)
                    .ToList();

                Server.Broadcast("<color=#ffcc00>[GunGame] ══ Round Over ══</color>");
                for (int i = 0; i < top3.Count; i++)
                {
                    var s = top3[i];
                    Server.Broadcast(
                        $"<color=#ffcc00>[GunGame]</color> #{i + 1} <color=#ffff00>{s.Name}</color>" +
                        $" — <color=#aaffaa>{s.Kills}K</color> / {s.Deaths}D | KDA: <color=#aaffaa>{s.KDA:F2}</color>");
                }

                foreach (var player in BasePlayer.activePlayerList)
                {
                    if (!_roundStats.TryGetValue(player.userID, out var ps)) continue;
                    var rank    = top3.FindIndex(s => s.SteamId == player.userID) + 1;
                    var rankStr = rank > 0 ? $" (#{rank})" : "";
                    player.ChatMessage(
                        $"<color=#aaffff>[GunGame] Your stats{rankStr}:</color>" +
                        $" {ps.Kills}K / {ps.Deaths}D | KDA: {ps.KDA:F2}");
                }
            }
        }

        private void CleanupDroppedItems()
        {
            var toRemove = new List<BaseNetworkable>();
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                if (entity is DroppedItem || entity is DroppedItemContainer)
                    toRemove.Add(entity);
            }
            foreach (var entity in toRemove)
                entity.Kill();
        }

        // ── Core ───────────────────────────────────────────────────────
        private void GiveGlobalWeapon(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            player.inventory.Strip();

            var loadout = _cfg.Loadouts[_weaponIndex];

            var weapon = ItemManager.CreateByName(loadout.Weapon, 1);
            if (weapon == null) return;

            if (weapon.contents != null)
            {
                foreach (var modSn in loadout.Attachments)
                {
                    var mod = ItemManager.CreateByName(modSn, 1);
                    if (mod != null && !mod.MoveToContainer(weapon.contents))
                        mod.Remove();
                }
            }

            if (!player.inventory.GiveItem(weapon, player.inventory.containerBelt))
            {
                weapon.Remove();
                return;
            }

            if (!string.IsNullOrEmpty(loadout.Ammo))
            {
                var ammo = ItemManager.CreateByName(loadout.Ammo, loadout.AmmoCount);
                if (ammo != null)
                    player.inventory.GiveItem(ammo, player.inventory.containerMain);
            }

            foreach (var wearSn in loadout.Wear)
            {
                var cloth = ItemManager.CreateByName(wearSn, 1);
                if (cloth == null) continue;

                if (!player.inventory.GiveItem(cloth, player.inventory.containerWear))
                    cloth.MoveToContainer(player.inventory.containerMain);
            }

            foreach (var extra in loadout.Extras)
            {
                var item = ItemManager.CreateByName(extra.Shortname, extra.Amount);
                if (item == null) continue;

                var target = extra.Container == "belt" ? player.inventory.containerBelt
                           : extra.Container == "wear" ? player.inventory.containerWear
                           : player.inventory.containerMain;

                if (!player.inventory.GiveItem(item, target))
                    item.MoveToContainer(player.inventory.containerMain);
            }
        }

        private RoundStats GetOrCreateStats(ulong steamId, string name)
        {
            if (!_roundStats.TryGetValue(steamId, out var stats))
            {
                stats = new RoundStats { SteamId = steamId, Name = name };
                _roundStats[steamId] = stats;
            }
            else
            {
                stats.Name = name;
            }
            return stats;
        }

        // ── Hooks ──────────────────────────────────────────────────────

        private void OnPlayerRespawned(BasePlayer player)
        {
            var pos = GetRandomSpawnPoint();
            if (pos != Vector3.zero)
                player.Teleport(pos);

            GiveGlobalWeapon(player);
        }

        private void OnPlayerConnected(BasePlayer player) => GiveGlobalWeapon(player);

        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (player.IsNpc) return;

            GetOrCreateStats(player.userID, player.displayName).Deaths++;

            var attacker = info?.InitiatorPlayer;
            if (attacker != null && !attacker.IsNpc && attacker.userID != player.userID)
            {
                var stats = GetOrCreateStats(attacker.userID, attacker.displayName);
                stats.Kills++;

                if (_activeMode == GameMode.KingOfTheHill)
                {
                    stats.KothScore += _cfg.KothKillPoints;
                    attacker.ChatMessage($"<color=#ffcc00>[KoTH]</color> +{_cfg.KothKillPoints} pts! Total: <color=#aaffaa>{stats.KothScore}</color>");
                }
            }
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity is TreeEntity || entity is OreResourceEntity || entity is LootContainer)
                info?.damageTypes?.ScaleAll(0f);
        }

        private object OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item) => false;
        private object OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item) => false;

        private object CanPickupItem(BasePlayer player, Item item) => false;

        private object OnCollectiblePickup(CollectibleEntity collectible, BasePlayer player) => false;

        private object CanLootEntity(BasePlayer player, BaseEntity entity)
        {
            // Allow vehicle fuel storage so players can refuel (lowgradefuel exception)
            if (entity is StorageContainer sc && sc.ShortPrefabName.Contains("fuel"))
                return null;

            if (entity is LootableCorpse || entity is DroppedItemContainer ||
                entity is LootContainer   || entity is StorageContainer)
                return false;

            return null;
        }

        private object CanLootPlayer(BasePlayer target, BasePlayer looter) => false;

        // ── Chat commands (admin only) ─────────────────────────────────
        [ChatCommand("gg")]
        private void CmdGg(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) { player.ChatMessage("[GunGame] Admins only."); return; }

            var sub = args.Length > 0 ? args[0].ToLower() : "help";

            if (sub == "help")
            {
                player.ChatMessage(
                    "<color=#ffcc00>[GunGame] ── Admin Commands ──────────────────</color>\n" +
                    "<color=#ffffff>/gg help</color>               – show this list\n" +
                    "<color=#ffffff>/gg mode gungame</color>       – switch to Gun Game mode\n" +
                    "<color=#ffffff>/gg mode koth</color>          – switch to King of the Hill mode\n" +
                    "<color=#ffffff>/gg hill</color>               – show current KoTH zone grid + coords\n" +
                    "<color=#ffffff>/gg spawn</color>              – regenerate spawn points\n" +
                    "<color=#aaffff>── Player Commands ──────────────────────────</color>\n" +
                    "<color=#ffffff>/stats</color>                 – live leaderboard (top 5)\n" +
                    $"<color=#aaaaaa>Current mode: {_activeMode} | World size: {World.Size}m</color>");
                return;
            }

            switch (sub)
            {
                case "mode":
                    if (args.Length < 2) { player.ChatMessage("Usage: /gg mode <gungame|koth>"); return; }

                    if (args[1].ToLower() == "koth" || args[1].ToLower() == "kingofthehill")
                    {
                        _activeMode  = GameMode.KingOfTheHill;
                        _cfg.Mode    = "KingOfTheHill";
                        SaveConfig();
                        StartKingOfTheHill();
                        Server.Broadcast("<color=#ffcc00>[GunGame]</color> Mode changed: <color=#ff6600>King of the Hill</color>!");
                    }
                    else
                    {
                        _activeMode = GameMode.GunGame;
                        _cfg.Mode   = "GunGame";
                        SaveConfig();
                        StopKingOfTheHill();
                        Server.Broadcast("<color=#ffcc00>[GunGame]</color> Mode changed: <color=#aaffaa>Gun Game</color>!");
                    }
                    break;

                case "hill":
                    if (_activeMode != GameMode.KingOfTheHill)
                        { player.ChatMessage("[GunGame] Only available in KoTH mode."); return; }
                    player.ChatMessage($"[KoTH] Zone: <color=#ff6600>{GetGridReference(_hillPosition)}</color> | Radius: {_cfg.KothRadius}m");
                    break;

                case "spawn":
                    GenerateSpawnPoints(60);
                    player.ChatMessage($"[GunGame] {_spawnPoints.Count} spawn points regenerated.");
                    break;

                default:
                    player.ChatMessage("[GunGame] Unknown command. Use /gg help for the command list.");
                    break;
            }
        }

        // ── Chat commands (everyone) ───────────────────────────────────
        [ChatCommand("stats")]
        private void CmdStats(BasePlayer player, string command, string[] args)
        {
            if (_roundStats.Count == 0)
            {
                player.ChatMessage("<color=#aaffff>[Stats]</color> No stats yet this round.");
                return;
            }

            if (_activeMode == GameMode.KingOfTheHill)
            {
                var sorted = _roundStats.Values
                    .OrderByDescending(s => s.KothScore)
                    .ToList();
                var top5 = sorted.Take(5).ToList();

                player.ChatMessage("<color=#ffcc00>[KoTH] ── Live Leaderboard ──────────</color>");
                for (int i = 0; i < top5.Count; i++)
                {
                    var s      = top5[i];
                    var marker = s.SteamId == player.userID ? " <color=#ff6600>◄</color>" : "";
                    player.ChatMessage(
                        $"<color=#ffcc00>#{i + 1}</color> <color=#ffff00>{s.Name}</color>" +
                        $" — <color=#aaffaa>{s.KothScore} pts</color> ({s.Kills}K / {s.Deaths}D){marker}");
                }

                var ownRank = sorted.FindIndex(s => s.SteamId == player.userID) + 1;
                if (ownRank > 5 && _roundStats.TryGetValue(player.userID, out var ps))
                    player.ChatMessage(
                        $"Your rank: <color=#aaffff>#{ownRank}</color>" +
                        $" — <color=#aaffaa>{ps.KothScore} pts</color> ({ps.Kills}K / {ps.Deaths}D) <color=#ff6600>◄</color>");
            }
            else
            {
                var sorted = _roundStats.Values
                    .OrderByDescending(s => s.Kills)
                    .ThenByDescending(s => s.KDA)
                    .ToList();
                var top5 = sorted.Take(5).ToList();

                player.ChatMessage("<color=#ffcc00>[GunGame] ── Live Leaderboard ──────</color>");
                for (int i = 0; i < top5.Count; i++)
                {
                    var s      = top5[i];
                    var marker = s.SteamId == player.userID ? " <color=#ff6600>◄</color>" : "";
                    player.ChatMessage(
                        $"<color=#ffcc00>#{i + 1}</color> <color=#ffff00>{s.Name}</color>" +
                        $" — <color=#aaffaa>{s.Kills}K</color> / {s.Deaths}D | KDA: {s.KDA:F2}{marker}");
                }

                var ownRank = sorted.FindIndex(s => s.SteamId == player.userID) + 1;
                if (ownRank > 5 && _roundStats.TryGetValue(player.userID, out var ps))
                    player.ChatMessage(
                        $"Your rank: <color=#aaffff>#{ownRank}</color>" +
                        $" — {ps.Kills}K / {ps.Deaths}D | KDA: {ps.KDA:F2} <color=#ff6600>◄</color>");
            }
        }

    }
}
