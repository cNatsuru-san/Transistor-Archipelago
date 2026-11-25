from typing import Dict, NamedTuple
from BaseClasses import Item, ItemClassification


class TransistorItem(Item):
    game: str = "Transistor"


class ItemData(NamedTuple):
    code: int
    classification: ItemClassification


FUNCTION_ITEMS = [
    "Crash()", "Breach()", "Jaunt()", "Spark()",
    "Bounce()", "Load()", "Switch()", "Mask()",
    "Ping()", "Help()", "Cull()", "Get()",
    "Purge()", "Tap()", "Flood()", "Void()",
]

KEY_ITEMS = [
    "Motorcycle Key",
    "Skyway Ferry Ticket",
    "Canal Boat Key",
]

LIMITER_ITEMS = [
    "Limiter: Risk()", "Limiter: Peace()", "Limiter: Time()",
    "Limiter: Cells()", "Limiter: Process()", "Limiter: User()",
    "Limiter: Void()", "Limiter: Overload()", "Limiter: Synergy()",
    "Limiter: Agency()",
]

ITEM_TABLE: Dict[str, ItemData] = {
    # 16 Funciones (Progression para que el generador respete los mínimos por región)
    "Crash()":  ItemData(78001, ItemClassification.progression),
    "Breach()": ItemData(78002, ItemClassification.progression),
    "Jaunt()":  ItemData(78003, ItemClassification.progression),
    "Spark()":  ItemData(78004, ItemClassification.progression),
    "Bounce()": ItemData(78005, ItemClassification.progression),
    "Load()":   ItemData(78006, ItemClassification.progression),
    "Switch()": ItemData(78007, ItemClassification.progression),
    "Mask()":   ItemData(78008, ItemClassification.progression),
    "Ping()":   ItemData(78009, ItemClassification.progression),
    "Help()":   ItemData(78010, ItemClassification.progression),
    "Cull()":   ItemData(78011, ItemClassification.progression),
    "Get()":    ItemData(78012, ItemClassification.progression),
    "Purge()":  ItemData(78013, ItemClassification.progression),
    "Tap()":    ItemData(78014, ItemClassification.progression),
    "Flood()":  ItemData(78015, ItemClassification.progression),
    "Void()":   ItemData(78016, ItemClassification.progression),

    # Llaves de progreso
    "Motorcycle Key":      ItemData(78051, ItemClassification.progression),
    "Skyway Ferry Ticket": ItemData(78052, ItemClassification.progression),
    "Canal Boat Key":      ItemData(78053, ItemClassification.progression),

    # Mejoras útiles (con tope en create_items)
    "+2 MEM Capacity":     ItemData(78061, ItemClassification.useful),
    "Passive Slot Unlock": ItemData(78062, ItemClassification.useful),

    # Limitadores
    "Limiter: Risk()":     ItemData(78071, ItemClassification.filler),
    "Limiter: Peace()":    ItemData(78072, ItemClassification.filler),
    "Limiter: Time()":     ItemData(78073, ItemClassification.filler),
    "Limiter: Cells()":    ItemData(78074, ItemClassification.filler),
    "Limiter: Process()":  ItemData(78075, ItemClassification.filler),
    "Limiter: User()":     ItemData(78076, ItemClassification.filler),
    "Limiter: Void()":     ItemData(78077, ItemClassification.filler),
    "Limiter: Overload()": ItemData(78078, ItemClassification.filler),
    "Limiter: Synergy()":  ItemData(78079, ItemClassification.filler),
    "Limiter: Agency()":   ItemData(78080, ItemClassification.filler),

    # Relleno neutro para no saturar de Passive Slot Unlock
    "Data Fragment":       ItemData(78099, ItemClassification.filler),
}
