from BaseClasses import Region, Tutorial
from worlds.AutoWorld import World, WebWorld
from .Items import TransistorItem, item_table
from .Locations import TransistorLocation, location_table
from .Options import TransistorOptions, Goal, EarlyVehicleKeys


class TransistorWeb(WebWorld):
    theme = "ice"
    tutorials = [Tutorial(
        "Multiworld Setup Guide",
        "Guia de configuracion para Transistor en Archipelago.",
        "Español",
        "setup_es.md",
        "setup/es",
        ["TransistorAP"]
    )]


class TransistorWorld(World):
    game = "Transistor"
    web = TransistorWeb()
    options_dataclass = TransistorOptions
    options: TransistorOptions

    item_name_to_id = {name: data.code for name, data in item_table.items()}
    location_name_to_id = {name: data.code for name, data in location_table.items()}

    def create_regions(self) -> None:
        menu = Region("Menu", self.player, self.multiworld)
        goldwalk = Region("Goldwalk", self.player, self.multiworld)
        highrise = Region("Highrise", self.player, self.multiworld)
        fairview = Region("Fairview", self.player, self.multiworld)
        canals = Region("Canals", self.player, self.multiworld)

        regiones = {
            "Goldwalk": goldwalk,
            "Highrise": highrise,
            "Fairview": fairview,
            "Canals": canals
        }

        for loc_name, loc_data in location_table.items():
            reg = regiones[loc_data.region]
            reg.locations.append(TransistorLocation(self.player, loc_name, loc_data.code, reg))

        menu.connect(goldwalk)
        goldwalk.connect(highrise, "Motorcycle Ride", lambda state: state.has("Motorcycle Key", self.player))
        highrise.connect(fairview, "Skyway Ferry", lambda state: state.has("Skyway Ferry Ticket", self.player))
        fairview.connect(canals, "Canal Boat", lambda state: state.has("Canal Boat Key", self.player))

        self.multiworld.regions.extend([menu, goldwalk, highrise, fairview, canals])

    def create_item(self, name: str) -> TransistorItem:
        data = item_table[name]
        return TransistorItem(name, data.classification, data.code, self.player)

    def create_items(self) -> None:
        pool = []
        keys_mode = self.options.vehicle_keys_mode.value
        llaves = ["Motorcycle Key", "Skyway Ferry Ticket", "Canal Boat Key"]

        for name, data in item_table.items():
            if data.category == "WEAPON":
                pool.append(self.create_item(name))
            elif data.category == "KEY":
                if keys_mode == EarlyVehicleKeys.option_start_with_keys:
                    self.multiworld.push_precollected(self.create_item(name))
                else:
                    pool.append(self.create_item(name))
            elif data.category == "LIMITER" and self.options.shuffle_limiters.value:
                pool.append(self.create_item(name))

        # Si el jugador eligio early_items (por defecto), garantizamos que las llaves salgan en esfera 1
        if keys_mode == EarlyVehicleKeys.option_early_items:
            for llave in llaves:
                self.multiworld.early_items[self.player][llave] = 1

        # Completamos hasta los 48 checks con mejoras de MEM y Pasivas
        total_checks = len(location_table)
        extras = ["+2 MEM Capacity", "Passive Slot Unlock"]
        i = 0
        while len(pool) < total_checks:
            pool.append(self.create_item(extras[i % len(extras)]))
            i += 1

        self.multiworld.itempool.extend(pool)

    def pre_fill(self) -> None:
        funciones_ofensivas = [
            "Crash()", "Breach()", "Spark()", "Bounce()", "Load()",
            "Cull()", "Get()", "Purge()", "Tap()", "Flood()"
        ]
        elegidas = self.random.sample(funciones_ofensivas, 2)

        for idx, loc_name in enumerate(["Starting Function 1 (Crash)", "Starting Function 2 (Breach)"]):
            nombre_func = elegidas[idx]
            for item in list(self.multiworld.itempool):
                if item.player == self.player and item.name == nombre_func:
                    self.multiworld.itempool.remove(item)
                    self.get_location(loc_name).place_locked_item(item)
                    break

    def set_rules(self) -> None:
        goal = self.options.goal.value
        if goal == Goal.option_all_functions:
            self.multiworld.completion_condition[self.player] = lambda state: all(
                state.has(name, self.player) for name, d in item_table.items() if d.category == "WEAPON"
            )
        else:
            self.multiworld.completion_condition[self.player] = (
                lambda state: state.can_reach("Boss - Royce Bracket", "Location", self.player)
            )