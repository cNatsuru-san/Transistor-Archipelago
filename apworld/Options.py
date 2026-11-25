from dataclasses import dataclass
from Options import Choice, Toggle, PerGameCommonOptions


class Goal(Choice):
    """Define la condición de victoria de la partida."""
    display_name = "Goal"
    option_royce = 0
    option_sandbox = 1
    option_max_level = 2
    default = 0


class IncludeSandbox(Toggle):
    """Añade las 25 pruebas del Sandbox (Puertas Traseras) como ubicaciones de checks.
    Se activa automáticamente si la meta elegida es 'sandbox'."""
    display_name = "Include Sandbox Checks"
    default = True


class RequireKeys(Toggle):
    """Randomiza la Llave de la Motocicleta, el Boleto del Ferry y la Llave del Bote.
    Si se desactiva, empiezas con las 3 llaves ya desbloqueadas."""
    display_name = "Require Region Keys"
    default = True


@dataclass
class TransistorOptions(PerGameCommonOptions):
    goal: Goal
    include_sandbox: IncludeSandbox
    require_keys: RequireKeys
