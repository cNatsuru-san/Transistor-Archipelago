# Transistor Archipelago Setup Guide / Guía de Instalación


---


### Required Software
- **[Archipelago](https://github.com/ArchipelagoMW/Archipelago/releases)** (v0.6.0 or newer).
- A legitimate PC copy of **Transistor** (Steam, GOG, or Epic Games).

### Step 1: Install the APWorld
1. Download **`transistor.apworld`** from the [Releases](../../releases) page.
2. Double-click `transistor.apworld` to install it automatically via Archipelago Launcher (or manually copy it into `C:\ProgramData\Archipelago\custom_worlds\`).
3. Open **Archipelago Launcher** and click **Generate Template Options**. This will create `Transistor.yaml` inside your `Players\Templates` folder.

### Step 2: Configure Your YAML
1. Copy `Transistor.yaml` into your Archipelago `Players` folder and open it with any text editor.
2. Adjust the options to your preference:
   - **`goal`**:
     - `royce`: Defeat the final boss (Royce Bracket) to complete the game.
     - `all_functions`: Collect all 16 Functions.
   - **`vehicle_keys_mode`**:
     - `early_items` *(Default — Recommended)*: Guarantees that transportation keys (`Motorcycle Key`, `Skyway Ferry Ticket`, `Canal Boat Key`) appear early in the seed, preventing softlocks in both solo and multiworld runs.
     - `start_with_keys`: Start the game with all 3 transportation keys already unlocked.
     - `full_random`: Keys can be placed anywhere in the multiworld without early restrictions.
   - **`shuffle_limiters`**: Set to `true` or `false` to include the 10 Limiters in the item pool.

### Step 3: Install the Game Client Mod
1. Download **`TransistorArchipelago-Client.zip`** from the [Releases](../../releases) page.
2. Locate your **Transistor** installation directory and open the **`x64`** folder (where `Transistor.exe` is located).
3. Extract all files from `TransistorArchipelago-Client.zip` (`TransistorArchipelago.dll`, `0Harmony.dll`, `Archipelago.MultiClient.Net.dll`, `archipelago_config.txt`, and the mod loader/injector) directly into the **`x64`** folder.

### Step 4: Connect and Play
1. Generate your multiworld game in Archipelago and host the room.
2. Open **`archipelago_config.txt`** inside your Transistor `x64` folder and enter your connection details on 3 separate lines:
   ```text
   archipelago.gg
   38281
   YourSlotName
(If playing locally on your own PC, use localhost on the first line).

3. Save archipelago_config.txt.
4. Launch Transistor using the mod loader/injector and start a New Game. You can verify your connection at any time by opening archipelago_log.txt inside the x64 folder.