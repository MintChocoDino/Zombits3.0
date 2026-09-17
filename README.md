# Zombits 3.0

A top-down 2D round-based zombie survival game built in **Unity 6 (6000.0.63f1)** with URP 2D.
Procedurally generated maps, escalating waves, a credit economy, perk machines, a mystery box
and Pack-a-Punch.

## Features

- **Procedural maps** — rooms, corridors, doors and interactable placement generated per run
  (`ProceduralMapGenerator`).
- **Wave scaling** — zombie count, health, speed and damage all scale per wave (`WaveManager`).
- **Grid pathfinding** — zombies path to the player over the tilemap (`GridPathfinder`).
- **Credit economy** — credits earned from damage dealt plus a kill bonus, spent on doors,
  perks, the mystery box and Pack-a-Punch.
- **Perks** — Juggernog, Speed Cola, Double Tap, Stamin-Up, Quick Revive.
- **Powerups** — Max Ammo, Insta-Kill, Nuke.
- **Multiplayer demo** — a separate scene using Netcode for GameObjects (see caveat below).

## Economy tuning

Income scales with zombie health so late waves stay worth farming:

```
credits per zombie = (maxHealth x creditsPerDamage) + creditsOnKill
                   = (maxHealth x 0.1) + 50
```

| Wave | Zombie HP | Zombies | Credits/zombie | Credits/wave |
|-----:|----------:|--------:|---------------:|-------------:|
| 1    | 100       | 6       | 60             | 360          |
| 5    | 220       | 22      | 72             | 1,584        |
| 10   | 370       | 42      | 87             | 3,654        |
| 15   | 520       | 62      | 102            | 6,324        |
| 18+  | 600 (cap) | 74      | 110            | 8,140        |

Prices:

| Purchase | Cost | Notes |
|---|---:|---|
| Starting credits | 500 | |
| Door | 250 | one per room entrance, many per map |
| Stamin-Up | 1,250 | +25% move speed |
| Speed Cola | 1,500 | 2x reload speed |
| Double Tap | 2,000 | +50% fire rate |
| Quick Revive | 2,000 | +5 HP/s regen |
| Juggernog | 2,500 | +100 max HP |
| Mystery Box | 950 | repeatable, random weapon |
| Pack-a-Punch | 5,000 | cost doubles after each use |

Target pacing: first perk around wave 3-4, Juggernog by wave 5-7, most perks by wave 10-12,
Pack-a-Punch around wave 12-14.

## Building

Opens in Unity 6 (6000.0.63f1). Build targets: Windows/Mac/Linux standalone, or WebGL.

**WebGL caveat:** `MultiplayerDemo` uses Unity Transport over UDP
(`m_UseWebSockets: 0`), which browsers do not allow. Either exclude that scene from the WebGL
build (and hide the Multiplayer button in the main menu), or switch the transport to WebSockets
and host a server with TLS.

## Project layout

```
Assets/
  Scripts/
    Game/            HUD, menus, interactables (doors, perks, box, Pack-a-Punch)
    Map Generation/  ProceduralMapGenerator
    Multiplayer/     Netcode for GameObjects demo
    Player Scripts/  PlayerState, controller, weapon holder
    Weapons/         Weapon base + Pistol/Rifle/Shotgun
    Zombie/          Zombie AI, spawner, pathfinding, powerups
  Prefabs/           Interactables, weapons, zombie, powerups
  Scenes/            MainMenu, Gameplay, MultiplayerDemo
```
