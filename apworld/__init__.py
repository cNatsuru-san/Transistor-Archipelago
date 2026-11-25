from typing import Dict, Any
from BaseClasses import Region
from worlds.AutoWorld import World
from worlds.generic.Rules import set_rule

from .items import (
    TransistorItem,
    ITEM_TABLE,
    FUNCTION_ITEMS,
    KEY_ITEMS,
    LIMITER_ITEMS,
)
from .locations import (
    TransistorLocation,
    LOCATION_TABLE,
    GOLDWALK_LOCATIONS,
    HIGHRISE_LOCATIONS,
    FAIRVIEW_LOCATIONS,
    ROOFTOPS_LOCATIONS,
    SHOWDOWN_LOCATIONS,
    SANDBOX_LOCATIONS,
)
from .options import TransistorOptions


class TransistorWorld(World):
    """Transistor Archipelago World."""
    game = "Transistor"
    options_dataclass = TransistorOptions
    options: TransistorOptions

    item_name_to_id = {name: data.code for name, data in ITEM_TABLE.items()}
    location_name_to_id = LOCATION_TABLE

    item_name_groups = {
        "Functions": set(FUNCTION_ITEMS),
        "Keys": set(KEY_ITEMS),
        "Limiters": set(LIMITER_ITEMS),
    }

    def _sandbox_enabled(self) -> bool:
        return bool(self.options.include_sandbox.value) or self.options.goal.value == 1

    def create_item(self, name: str) -> TransistorItem:
        data = ITEM_TABLE[name]
        return TransistorItem(name, data.classification, data.code, self.player)

    def create_regions(self) -> None:
        menu = Region("Menu", self.player, self.multiworld)
        goldwalk = Region("Goldwalk", self.player, self.multiworld)
        highrise = Region("Highrise", self.player, self.multiworld)
        fairview = Region("Fairview", self.player, self.multiworld)
        rooftops = Region("Rooftops", self.player, self.multiworld)
        showdown = Region("Showdown", self.player, self.multiworld)

        self.multiworld.regions.extend([menu, goldwalk, highrise, fairview, rooftops, showdown])

        for name, loc_id in GOLDWALK_LOCATIONS.items():
            goldwalk.locations.append(TransistorLocation(self.player, name, loc_id, goldwalk))
        for name, loc_id in HIGHRISE_LOCATIONS.items():
            highrise.locations.append(TransistorLocation(self.player, name, loc_id, highrise))
        for name, loc_id in FAIRVIEW_LOCATIONS.items():
            fairview.locations.append(TransistorLocation(self.player, name, loc_id, fairview))
        for name, loc_id in ROOFTOPS_LOCATIONS.items():
            rooftops.locations.append(TransistorLocation(self.player, name, loc_id, rooftops))
        for name, loc_id in SHOWDOWN_LOCATIONS.items():
            showdown.locations.append(TransistorLocation(self.player, name, loc_id, showdown))

        menu.connect(goldwalk)
        goldwalk.connect(highrise, "Goldwalk to Highrise")
        highrise.connect(fairview, "Highrise to Fairview")
        fairview.connect(rooftops, "Fairview to Rooftops")
        rooftops.connect(showdown, "Rooftops to Showdown")

        if self._sandbox_enabled():
            sandbox = Region("Sandbox", self.player, self.multiworld)
            self.multiworld.regions.append(sandbox)
            for name, loc_id in SANDBOX_LOCATIONS.items():
                sandbox.locations.append(TransistorLocation(self.player, name, loc_id, sandbox))
            # El acceso al Sandbox (Puertas Traseras) se abre tras Highrise / Sybil
            highrise.connect(sandbox, "Highrise to Sandbox")

    def create_items(self) -> None:
        pool = []

        # 2 funciones iniciales garantizadas al comenzar
        starting_funcs = self.random.sample(FUNCTION_ITEMS, 2)
        for func in starting_funcs:
            self.multiworld.push_precollected(self.create_item(func))

        # Las otras 14 funciones entran al pool
        for func in FUNCTION_ITEMS:
            if func not in starting_funcs:
                pool.append(self.create_item(func))

       # 3 llaves de progreso (randomizadas o pre-entregadas según options.require_keys)
        for key in KEY_ITEMS:
            if self.options.require_keys.value:
                pool.append(self.create_item(key))
            else:
                self.multiworld.push_precollected(self.create_item(key))

        # Máximo 4 Passive Slot Unlock (límite real del juego)
        for _ in range(4):
            pool.append(self.create_item("Passive Slot Unlock"))

        # 8 aumentos de +2 MEM (de 16 hasta 32 MEM)
        for _ in range(8):
            pool.append(self.create_item("+2 MEM Capacity"))

        # 10 Limitadores
        for lim in LIMITER_ITEMS:
            pool.append(self.create_item(lim))

        # Completar exactamente la cantidad de ubicaciones con relleno neutro
        total_locations = len(self.multiworld.get_unfilled_locations(self.player))
        while len(pool) < total_locations:
            pool.append(self.create_item("Data Fragment"))

        self.multiworld.itempool.extend(pool[:total_locations])

    def set_rules(self) -> None:
        # Reglas de llaves + mínimo de funciones por región
        set_rule(
            self.multiworld.get_entrance("Goldwalk to Highrise", self.player),
            lambda state: state.has("Motorcycle Key", self.player)
            and state.has_group("Functions", self.player, 4),
        )
        set_rule(
            self.multiworld.get_entrance("Highrise to Fairview", self.player),
            lambda state: state.has("Skyway Ferry Ticket", self.player)
            and state.has_group("Functions", self.player, 7),
        )
        set_rule(
            self.multiworld.get_entrance("Fairview to Rooftops", self.player),
            lambda state: state.has("Canal Boat Key", self.player)
            and state.has_group("Functions", self.player, 10),
        )
        set_rule(
            self.multiworld.get_entrance("Rooftops to Showdown", self.player),
            lambda state: state.has_group("Functions", self.player, 12),
        )

        if self._sandbox_enabled():
            set_rule(
                self.multiworld.get_entrance("Highrise to Sandbox", self.player),
                lambda state: state.has_group("Functions", self.player, 6),
            )
            # Pruebas finales del Sandbox requieren al menos 12 funciones
            for test_name in [
                "Sandbox - Speed Test 5",
                "Sandbox - Stability Test 5",
                "Sandbox - Planning Test 5",
                "Sandbox - Performance Test 7",
                "Sandbox - Agency Test 3",
            ]:
                set_rule(
                    self.multiworld.get_location(test_name, self.player),
                    lambda state: state.has_group("Functions", self.player, 12),
                )

        # Condición de victoria lógica según la meta elegida
        goal = self.options.goal.value
        if goal == 1:  # Sandbox
            self.multiworld.completion_condition[self.player] = (
                lambda state: state.can_reach("Sandbox", "Region", self.player)
                and state.has_group("Functions", self.player, 12)
            )
        elif goal == 2:  # Max Level
            self.multiworld.completion_condition[self.player] = (
                lambda state: state.can_reach("Showdown", "Region", self.player)
            )
        else:  # Royce Bracket
            self.multiworld.completion_condition[self.player] = (
                lambda state: state.can_reach("Showdown", "Region", self.player)
            )

    def fill_slot_data(self) -> Dict[str, Any]:
        return {
            "goal": self.options.goal.value,
            "include_sandbox": self._sandbox_enabled(),
            "require_keys": bool(self.options.require_keys.value),
        }
