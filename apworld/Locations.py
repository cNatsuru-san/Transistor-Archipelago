from typing import Dict
from BaseClasses import Location


class TransistorLocation(Location):
    game: str = "Transistor"


# Ubicaciones por región
GOLDWALK_LOCATIONS: Dict[str, int] = {
    "Starting Function 1 (Crash)": 78001,
    "Starting Function 2 (Breach)": 78002,
    "Goldwalk - Jaunt()": 78003,
    "Goldwalk - Spark()": 78004,
    "Goldwalk - Mask()": 78008,
    "Level Up 2 - Option A": 78202,
    "Level Up 2 - Option B": 78302,
    "Level Up 3 - Option A": 78203,
    "Level Up 3 - Option B": 78303,
}

HIGHRISE_LOCATIONS: Dict[str, int] = {
    "Highrise - Bounce()": 78005,
    "Highrise - Load()": 78006,
    "Highrise - Switch()": 78007,
    "Highrise - Ping()": 78009,
    "Highrise - Help()": 78010,
    "Boss - Sybil Reisz": 78101,
    "Level Up 4 - Option A": 78204,
    "Level Up 4 - Option B": 78304,
    "Level Up 5 - Option A": 78205,
    "Level Up 5 - Option B": 78305,
    "Level Up 6 - Option A": 78206,
    "Level Up 6 - Option B": 78306,
}

FAIRVIEW_LOCATIONS: Dict[str, int] = {
    "Fairview - Cull()": 78011,
    "Fairview - Get()": 78012,
    "Fairview - Purge()": 78013,
    "Fairview - Tap()": 78014,
    "Canals - Flood()": 78015,
    "Canals - Void()": 78016,
    "Boss - The Spine": 78102,
    "Level Up 7 - Option A": 78207,
    "Level Up 7 - Option B": 78307,
    "Level Up 8 - Option A": 78208,
    "Level Up 8 - Option B": 78308,
    "Level Up 9 - Option A": 78209,
    "Level Up 9 - Option B": 78309,
    "Level Up 10 - Option A": 78210,
    "Level Up 10 - Option B": 78310,
}

ROOFTOPS_LOCATIONS: Dict[str, int] = {
    "Boss - Asher & Grant": 78103,
    "Level Up 11 - Option A": 78211,
    "Level Up 11 - Option B": 78311,
    "Level Up 12 - Option A": 78212,
    "Level Up 12 - Option B": 78312,
    "Level Up 13 - Option A": 78213,
    "Level Up 13 - Option B": 78313,
}

SHOWDOWN_LOCATIONS: Dict[str, int] = {
    "Boss - Royce Bracket": 78104,
    "Level Up 14 - Option A": 78214,
    "Level Up 14 - Option B": 78314,
    "Level Up 15 - Option A": 78215,
    "Level Up 15 - Option B": 78315,
}

SANDBOX_LOCATIONS: Dict[str, int] = {
    # Speed Tests (5)
    "Sandbox - Speed Test 1": 78401,
    "Sandbox - Speed Test 2": 78402,
    "Sandbox - Speed Test 3": 78403,
    "Sandbox - Speed Test 4": 78404,
    "Sandbox - Speed Test 5": 78405,
    # Stability Tests (5)
    "Sandbox - Stability Test 1": 78406,
    "Sandbox - Stability Test 2": 78407,
    "Sandbox - Stability Test 3": 78408,
    "Sandbox - Stability Test 4": 78409,
    "Sandbox - Stability Test 5": 78410,
    # Planning Tests (5)
    "Sandbox - Planning Test 1": 78411,
    "Sandbox - Planning Test 2": 78412,
    "Sandbox - Planning Test 3": 78413,
    "Sandbox - Planning Test 4": 78414,
    "Sandbox - Planning Test 5": 78415,
    # Performance Tests (7)
    "Sandbox - Performance Test 1": 78416,
    "Sandbox - Performance Test 2": 78417,
    "Sandbox - Performance Test 3": 78418,
    "Sandbox - Performance Test 4": 78419,
    "Sandbox - Performance Test 5": 78420,
    "Sandbox - Performance Test 6": 78421,
    "Sandbox - Performance Test 7": 78422,
    # Agency Tests (3)
    "Sandbox - Agency Test 1": 78423,
    "Sandbox - Agency Test 2": 78424,
    "Sandbox - Agency Test 3": 78425,
}

LOCATION_TABLE: Dict[str, int] = {
    **GOLDWALK_LOCATIONS,
    **HIGHRISE_LOCATIONS,
    **FAIRVIEW_LOCATIONS,
    **ROOFTOPS_LOCATIONS,
    **SHOWDOWN_LOCATIONS,
    **SANDBOX_LOCATIONS,
}
