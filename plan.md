Feladat: Készíts egy átfogó technikai dokumentációt és egy kezdeti kódstruktúrát (Boilerplate) egy teljesen önálló, zárt "Arena GunGame" Rust pluginhoz.

Tervezési alapelv: Ez a plugin teljesen izoláltan, különállóan működik. Semmilyen más pluginra (pl. anti-cheat, EventManager, Kits, Spawns) nincs függősége, és nem kommunikál más rendszerekkel. Minden logikát natív módon, önmagában old meg.

Funkcionális követelmények (Arena Mode):

Zero Dependencies: Kizárólag natív Rust API-t használhatsz (pl. ItemManager, BasePlayer, ResourceEntity).

Globális fegyverciklus: Nem egyéni ölések alapján haladnak a játékosok. Egy központi Timer (beállíthatóan pl. 3-5 perc) fut, és amikor lejár, a szerver az ÖSSZES online játékos fegyverét egyszerre váltja le a következőre.

Világ lezárása (Sandbox/Arena): Meg kell akadályozni minden környezeti interakciót. A fák, kövek, hordók, ládák sebezhetetlenek (nem üthetők ki, nem adnak nyersanyagot). A lootolás (hullák, droppolt táskák) teljesen le van tiltva.

Spawn kezelés: Ha egy játékos meghal, az újjászületéskor (OnPlayerRespawn) automatikusan meg kell kapnia az aktuális globális fegyvert és lőszert, üres inventory-val.

A Dokumentáció tartalmazza:

Az alkalmazott Rust Hook-ok listáját és magyarázatát (OnEntityTakeDamage, CanLootEntity, OnPlayerRespawn / OnPlayerConnected).

A globális állapot (aktuális fegyver index) kezelésének menetét.

A Kódstruktúra tartalmazza:

A Carbon-kompatibilis alap osztályvázat a namespace-ekkel.

Egy robusztus GiveGlobalWeapon(BasePlayer player) metódust, ami üríti a játékos táskáját (player.inventory.Strip()), a belt-be rakja a fegyvert, és automatikusan ad hozzá megfelelő típusú lőszert.

A ciklust vezérlő timer.Repeat implementációt.

Stílus: Tömör, fejlesztőknek szóló, lényegre törő technikai szöveg, felesleges magyarázatok nélkül.