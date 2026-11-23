from typing import Dict, NamedTuple
from BaseClasses import Item, ItemClassification

class TransistorItem(Item):
    game: str = "Transistor"

class ItemData(NamedTuple):
    code: int
    classification: ItemClassification
    internal_name: str
    category: str

item_table: Dict[str, ItemData] = {
    # 16 Funciones
    "Crash()":  ItemData(77001, ItemClassification.progression, "Slam",     "WEAPON"),
    "Breach()": ItemData(77002, ItemClassification.progression, "Snipe",    "WEAPON"),
    "Jaunt()":  ItemData(77003, ItemClassification.progression, "Blink",    "WEAPON"),
    "Spark()":  ItemData(77004, ItemClassification.progression, "Bomb",     "WEAPON"),
    "Bounce()": ItemData(77005, ItemClassification.progression, "Orb",      "WEAPON"),
    "Load()":   ItemData(77006, ItemClassification.progression, "Launch",   "WEAPON"),
    "Switch()": ItemData(77007, ItemClassification.progression, "Charm",    "WEAPON"),
    "Mask()":   ItemData(77008, ItemClassification.progression, "Hide",     "WEAPON"),
    "Ping()":   ItemData(77009, ItemClassification.progression, "Sidearm",  "WEAPON"),
    "Help()":   ItemData(77010, ItemClassification.progression, "Summon",   "WEAPON"),
    "Cull()":   ItemData(77011, ItemClassification.progression, "Uppercut", "WEAPON"),
    "Get()":    ItemData(77012, ItemClassification.progression, "Hook",     "WEAPON"),
    "Purge()":  ItemData(77013, ItemClassification.progression, "Tracker",  "WEAPON"),
    "Tap()":    ItemData(77014, ItemClassification.progression, "Heal",     "WEAPON"),
    "Flood()":  ItemData(77015, ItemClassification.progression, "Flusher",  "WEAPON"),
    "Void()":   ItemData(77016, ItemClassification.progression, "Might",    "WEAPON"),

    # 3 Llaves de Vehiculos (Progresion entre capitulos)
    "Motorcycle Key":      ItemData(77020, ItemClassification.progression, "KEY_MOTO",  "KEY"),
    "Skyway Ferry Ticket": ItemData(77021, ItemClassification.progression, "KEY_FERRY", "KEY"),
    "Canal Boat Key":      ItemData(77022, ItemClassification.progression, "KEY_BOAT",  "KEY"),

    # 10 Limitadores
    "Limiter: Peace()":       ItemData(77030, ItemClassification.useful, "LimiterPeace",       "LIMITER"),
    "Limiter: Void()":        ItemData(77031, ItemClassification.useful, "LimiterVoid",        "LIMITER"),
    "Limiter: Cells()":       ItemData(77032, ItemClassification.useful, "LimiterCells",       "LIMITER"),
    "Limiter: Process()":     ItemData(77033, ItemClassification.useful, "LimiterProcess",     "LIMITER"),
    "Limiter: Choice()":      ItemData(77034, ItemClassification.useful, "LimiterChoice",      "LIMITER"),
    "Limiter: Performance()": ItemData(77035, ItemClassification.useful, "LimiterPerformance", "LIMITER"),
    "Limiter: Time()":        ItemData(77036, ItemClassification.useful, "LimiterTime",        "LIMITER"),
    "Limiter: Function()":    ItemData(77037, ItemClassification.useful, "LimiterFunction",    "LIMITER"),
    "Limiter: User()":        ItemData(77038, ItemClassification.useful, "LimiterUser",        "LIMITER"),
    "Limiter: Risk()":        ItemData(77039, ItemClassification.useful, "LimiterRisk",        "LIMITER"),

    # Relleno util (MEM y Pasivas para cuadrar el total de checks)
    "+2 MEM Capacity":        ItemData(77050, ItemClassification.useful, "MEM_PLUS_2",   "UPGRADE"),
    "Passive Slot Unlock":    ItemData(77051, ItemClassification.useful, "PASSIVE_SLOT", "UPGRADE"),
}