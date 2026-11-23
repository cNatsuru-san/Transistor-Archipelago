from typing import Dict, NamedTuple
from BaseClasses import Location


class TransistorLocation(Location):
    game: str = "Transistor"


class LocationData(NamedTuple):
    code: int
    region: str


location_table: Dict[str, LocationData] = {
    # Funciones iniciales (Goldwalk)
    "Starting Function 1 (Crash)": LocationData(78001, "Goldwalk"),
    "Starting Function 2 (Breach)": LocationData(78002, "Goldwalk"),

    # Cuerpos / Funciones absorbibles
    "Goldwalk - Jaunt()": LocationData(78003, "Goldwalk"),
    "Goldwalk - Spark()": LocationData(78004, "Goldwalk"),
    "Goldwalk - Bounce()": LocationData(78005, "Goldwalk"),
    "Goldwalk - Load()": LocationData(78006, "Goldwalk"),
    "Goldwalk - Mask()": LocationData(78008, "Goldwalk"),
    "Highrise - Switch()": LocationData(78007, "Highrise"),
    "Highrise - Ping()": LocationData(78009, "Highrise"),
    "Highrise - Help()": LocationData(78010, "Highrise"),
    "Fairview - Cull()": LocationData(78011, "Fairview"),
    "Fairview - Get()": LocationData(78012, "Fairview"),
    "Fairview - Purge()": LocationData(78013, "Fairview"),
    "Canals - Tap()": LocationData(78014, "Canals"),
    "Canals - Flood()": LocationData(78015, "Canals"),
    "Canals - Void()": LocationData(78016, "Canals"),

    # Jefes
    "Boss - Sybil Reisz": LocationData(78101, "Highrise"),
    "Boss - The Spine": LocationData(78102, "Fairview"),
    "Boss - Asher & Grant": LocationData(78103, "Canals"),
    "Boss - Royce Bracket": LocationData(78104, "Canals"),

    # Subidas de Nivel - Opcion A (Niveles 2 al 15)
    "Level Up 2 - Option A": LocationData(78202, "Goldwalk"),
    "Level Up 3 - Option A": LocationData(78203, "Goldwalk"),
    "Level Up 4 - Option A": LocationData(78204, "Goldwalk"),
    "Level Up 5 - Option A": LocationData(78205, "Highrise"),
    "Level Up 6 - Option A": LocationData(78206, "Highrise"),
    "Level Up 7 - Option A": LocationData(78207, "Highrise"),
    "Level Up 8 - Option A": LocationData(78208, "Fairview"),
    "Level Up 9 - Option A": LocationData(78209, "Fairview"),
    "Level Up 10 - Option A": LocationData(78210, "Fairview"),
    "Level Up 11 - Option A": LocationData(78211, "Canals"),
    "Level Up 12 - Option A": LocationData(78212, "Canals"),
    "Level Up 13 - Option A": LocationData(78213, "Canals"),
    "Level Up 14 - Option A": LocationData(78214, "Canals"),
    "Level Up 15 - Option A": LocationData(78215, "Canals"),

    # Subidas de Nivel - Opcion B (Niveles 2 al 15)
    "Level Up 2 - Option B": LocationData(78302, "Goldwalk"),
    "Level Up 3 - Option B": LocationData(78303, "Goldwalk"),
    "Level Up 4 - Option B": LocationData(78304, "Goldwalk"),
    "Level Up 5 - Option B": LocationData(78305, "Highrise"),
    "Level Up 6 - Option B": LocationData(78306, "Highrise"),
    "Level Up 7 - Option B": LocationData(78307, "Highrise"),
    "Level Up 8 - Option B": LocationData(78308, "Fairview"),
    "Level Up 9 - Option B": LocationData(78309, "Fairview"),
    "Level Up 10 - Option B": LocationData(78310, "Fairview"),
    "Level Up 11 - Option B": LocationData(78311, "Canals"),
    "Level Up 12 - Option B": LocationData(78312, "Canals"),
    "Level Up 13 - Option B": LocationData(78313, "Canals"),
    "Level Up 14 - Option B": LocationData(78314, "Canals"),
    "Level Up 15 - Option B": LocationData(78315, "Canals"),
}