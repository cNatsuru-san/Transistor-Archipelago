from dataclasses import dataclass
from Options import Choice, Toggle, PerGameCommonOptions


class Goal(Choice):
    """Define la condicion de victoria para completar Transistor."""
    display_name = "Goal"
    option_royce = 0
    option_all_functions = 1
    default = 0


class EarlyVehicleKeys(Choice):
    """Controla como se distribuyen las llaves de transporte (Motorcycle Key, Skyway Ferry Ticket, Canal Boat Key).
    - early_items: Garantiza que aparezcan en el inicio de la partida (ideal para jugar solo o en grupo sin atascos).
    - start_with_keys: Empiezas directamente con todas las llaves de vehiculos desbloqueadas.
    - full_random: Pueden aparecer en cualquier lugar del multiworld sin restriccion."""
    display_name = "Vehicle Keys Mode"
    option_early_items = 0
    option_start_with_keys = 1
    option_full_random = 2
    default = 0


class ShuffleLimiters(Toggle):
    """Incluye los 10 Limitadores en la piscina de objetos aleatorios."""
    display_name = "Shuffle Limiters"
    default = 1


@dataclass
class TransistorOptions(PerGameCommonOptions):
    goal: Goal
    vehicle_keys_mode: EarlyVehicleKeys
    shuffle_limiters: ShuffleLimiters