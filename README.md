# Transistor Archipelago Randomizer (`v0.3.39`)
Este es un randomizer para transistor mi juego favorito asi que si te sirve o solamente te gusta el juego estaria muy feliz de que lo probaras

Como aclaracion este randomizer fue crado con inteligencia artificial como herramienta ya que no se programar pero queria hacer un randomizer / archipelago de mi juego favorito, por si llegan a haber errores, fallos o cosas fuera de mi control agradeceria que me lo hicieran saber para poder corregirlos

si es que no te agrada mucho como lo hice no hay mucho que pueda hacer la verdad ya que no se programar asi que lo siento por los inconveniente

por lo demas espero que lo disfruten asi como yo me diverti en hacerlo


C# client mod and Archipelago world (`.apworld`) to play **Transistor** as a Multiworld Randomizer.

## Features
* **Randomized Functions & Progression:** All 16 of Red's Functions, region keys, `+2 MEM Capacity` upgrades, Passive Slot Unlocks, and Limiters are shuffled into the multiworld.
* **3 Selectable Goals:** Defeat Royce Bracket, complete all 25 Sandbox (Backdoor) tests, or reach Max Level.
* **Sandbox Checks:** 25 additional locations across Speed, Stability, Planning, Performance, and Agency Tests.
* **Balanced Progression:** Minimum function requirements per region (4, 7, 10, and 12 functions) to prevent reaching late-game bosses with too few abilities.
* **Save File Stability:** Automatic level syncing with the Archipelago server and protection against false checks when continuing a saved game.

---

## Installation

### 1. Archipelago Setup
1. Download `transistor.apworld` and `Transistor.yaml` from the **Releases** page.
2. Place `transistor.apworld` inside your Archipelago `custom_worlds` folder.
3. Place `Transistor.yaml` inside your `Players` folder, configure your options, and generate your seed with `ArchipelagoGenerate.exe`.

### 2. Transistor Mod Setup
1. Download `TransistorArchipelago.dll` from **Releases** and place it in your Transistor `x64` folder (where `Transistor.exe` is located).
2. Create or edit `archipelago_config.txt` in the same `x64` folder with 3 lines (server address, port, and slot name):
   ```text
   localhost
   38281
   Red
## Options

### 1. Goals 
royce, sandbox, max level (Sets the victory condition: defeat Royce Bracket (royce), clear all 25 Sandbox tests (sandbox), or reach Level 20 (max_level))

### 2. include sandbox
Adds all 25 Sandbox tests as check locations. Automatically enabled when goal is set to sandbox

### 3. require keys
When true, randomizes Motorcycle Key, Skyway Ferry Ticket, and Canal Boat Key to unlock new areas. When false, you start with all 3 keys unlocked.

Notes:
* Connection status, sent checks, and received items are logged in real time to x64/archipelago_log.txt.
* Always close Transistor completely before replacing TransistorArchipelago.dll when updating the mod.


## Known Issues & Feedback / Reporte de Errores
This project is currently in **Beta**. If you encounter any softlocks, crashes, or logic issues, please open an [Issue](../../issues) and attach your `archipelago_log.txt` file from the `x64` folder.

Espero y lo disfrutes
