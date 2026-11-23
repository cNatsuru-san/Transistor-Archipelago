using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using GSGE.Code.GUI;
using GSGE.Code.Players;
using GSGE.Code.Things;
using HarmonyLib;

namespace TransistorArchipelago
{
    public class ModPrincipal
    {
        private static bool yaIniciado = false;
        public static ArchipelagoSession sesion;
        private static int itemsEntregados = 0;
        public static bool entregandoDesdeAP = false;
        private static bool victoriaEnviada = false;
        public static int nivelJugadorActual = 1;

        private static readonly Queue<string> colaItemsPendientes = new Queue<string>();
        private static readonly object bloqueoCola = new object();

        // Estado de llaves de vehículos/transporte
        public static bool tieneLlaveMoto = false;
        public static bool tieneTicketFerry = false;
        public static bool tieneLlaveBote = false;

        private static readonly HashSet<string> cartasPermitidas = new HashSet<string>();
        private static readonly HashSet<long> checksEnviados = new HashSet<long>();

        public static string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
        public static string rutaLog => Path.Combine(rutaBase, "archipelago_log.txt");

        public static readonly Dictionary<string, string> mapaHabilidades = new Dictionary<string, string>
        {
            { "Crash()",  "Slam" },
            { "Breach()", "Snipe" },
            { "Jaunt()",  "Blink" },
            { "Spark()",  "Bomb" },
            { "Bounce()", "Orb" },
            { "Load()",   "Launch" },
            { "Switch()", "Charm" },
            { "Mask()",   "Hide" },
            { "Ping()",   "Sidearm" },
            { "Help()",   "Summon" },
            { "Cull()",   "Uppercut" },
            { "Get()",    "Hook" },
            { "Purge()",  "Tracker" },
            { "Tap()",    "Heal" },
            { "Flood()",  "Flusher" },
            { "Void()",   "Might" }
        };

        public static readonly Dictionary<string, long> mapaChecksPorCarta = new Dictionary<string, long>
        {
            { "Slam",     78001 },
            { "Snipe",    78002 },
            { "Blink",    78003 },
            { "Bomb",     78004 },
            { "Orb",      78005 },
            { "Launch",   78006 },
            { "Charm",    78007 },
            { "Hide",     78008 },
            { "Sidearm",  78009 },
            { "Summon",   78010 },
            { "Uppercut", 78011 },
            { "Hook",     78012 },
            { "Tracker",  78013 },
            { "Heal",     78014 },
            { "Flusher",  78015 },
            { "Might",    78016 }
        };

        public static void Iniciar()
        {
            if (yaIniciado) return;
            yaIniciado = true;

            try
            {
                Harmony harmony = new Harmony("com.transistor.archipelago");
                harmony.PatchAll();
                File.WriteAllText(rutaLog, "Transistor Archipelago v3.1 (Completo: 48 Checks + Llaves + Jefes) iniciado.\n");
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(rutaBase, "archipelago_harmony_error.txt"), ex.ToString());
            }

            Thread hiloMod = new Thread(BuclePrincipal);
            hiloMod.IsBackground = true;
            hiloMod.Start();
        }

        public static void EnviarCheck(long idCheck, string descripcion)
        {
            if (checksEnviados.Contains(idCheck)) return;
            checksEnviados.Add(idCheck);

            try
            {
                if (sesion != null && sesion.Socket.Connected)
                {
                    sesion.Locations.CompleteLocationChecks(idCheck);
                    File.AppendAllText(rutaLog, $"[CHECK ENVIADO] {descripcion} -> ID {idCheck}\n");
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(rutaBase, "archipelago_error.txt"), ex.ToString());
            }
        }

        public static void EnviarVictoria(string motivo)
        {
            if (victoriaEnviada) return;
            victoriaEnviada = true;

            try
            {
                if (sesion != null && sesion.Socket.Connected)
                {
                    var packet = new StatusUpdatePacket { Status = ArchipelagoClientState.ClientGoal };
                    sesion.Socket.SendPacket(packet);
                    File.AppendAllText(rutaLog, $"[VICTORIA] ¡Meta alcanzada ({motivo})! Enviado a Archipelago.\n");
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(rutaBase, "archipelago_error.txt"), ex.ToString());
            }
        }

        private static void EquiparCartaEnBarra(Deck deck, Card carta)
        {
            if (deck == null || carta == null) return;
            try
            {
                int slotLibre = deck.getFirstEmptySlot();
                if (slotLibre >= 0 && slotLibre <= 1)
                {
                    deck.DrawCard(carta);
                    deck.playCard(carta, slotLibre, true);
                    File.AppendAllText(rutaLog, $"[EQUIPADO] '{carta.Name}' equipada en Active[{slotLibre}].\n");
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(rutaLog, $"[ERROR EQUIPAR] {ex.Message}\n");
            }
        }

        public static void ProcesarColaEnHiloPrincipal()
        {
            Player jugador = PlayerManager.getPlayer();
            if (jugador == null || jugador.Deck == null) return;

            lock (bloqueoCola)
            {
                while (colaItemsPendientes.Count > 0)
                {
                    string nombreAP = colaItemsPendientes.Dequeue();

                    if (mapaHabilidades.ContainsKey(nombreAP))
                    {
                        string nombreInterno = mapaHabilidades[nombreAP];
                        cartasPermitidas.Add(nombreInterno);

                        entregandoDesdeAP = true;
                        Card nuevaCarta = jugador.Deck.createCard(nombreInterno, Card.CardType.WEAPON);
                        if (nuevaCarta != null && jugador.Deck.CalcNumActivePowers() < 2)
                        {
                            EquiparCartaEnBarra(jugador.Deck, nuevaCarta);
                        }
                        entregandoDesdeAP = false;

                        File.AppendAllText(rutaLog, $"[RECIBIDO] Funcion {nombreAP} -> {nombreInterno}\n");
                    }
                    else if (nombreAP == "Motorcycle Key")
                    {
                        tieneLlaveMoto = true;
                        File.AppendAllText(rutaLog, "[RECIBIDO] ¡Llave de la Motocicleta desbloqueada!\n");
                    }
                    else if (nombreAP == "Skyway Ferry Ticket")
                    {
                        tieneTicketFerry = true;
                        File.AppendAllText(rutaLog, "[RECIBIDO] ¡Boleto del Ferry/Planeador desbloqueado!\n");
                    }
                    else if (nombreAP == "Canal Boat Key")
                    {
                        tieneLlaveBote = true;
                        File.AppendAllText(rutaLog, "[RECIBIDO] ¡Llave del Bote de los Canales desbloqueada!\n");
                    }
                    else if (nombreAP.StartsWith("Limiter:"))
                    {
                        File.AppendAllText(rutaLog, $"[RECIBIDO] Limitador recibido: {nombreAP}\n");
                    }
                    else if (nombreAP == "+2 MEM Capacity")
                    {
                        jugador.Deck.AdjustMaxActiveSize(2);
                        File.AppendAllText(rutaLog, "[RECIBIDO] +2 MEM aplicado al mazo.\n");
                    }
                    else if (nombreAP == "Passive Slot Unlock")
                    {
                        File.AppendAllText(rutaLog, "[RECIBIDO] Ranura Pasiva desbloqueada.\n");
                    }
                }
            }

            if (jugador.Deck.CalcNumActivePowers() < 2 && jugador.Deck.Library != null && jugador.Deck.Library.Count > 0)
            {
                Card siguienteCarta = jugador.Deck.Library[0];
                if (siguienteCarta != null)
                {
                    EquiparCartaEnBarra(jugador.Deck, siguienteCarta);
                }
            }
        }

        private static void BuclePrincipal()
        {
            try
            {
                string servidor = "localhost";
                int puerto = 38281;
                string nombreJugador = "Red";
                string rutaConfig = Path.Combine(rutaBase, "archipelago_config.txt");

                if (File.Exists(rutaConfig))
                {
                    string[] lineas = File.ReadAllLines(rutaConfig);
                    if (lineas.Length >= 3)
                    {
                        servidor = lineas[0].Trim();
                        int.TryParse(lineas[1].Trim(), out puerto);
                        nombreJugador = lineas[2].Trim();
                    }
                }

                sesion = ArchipelagoSessionFactory.CreateSession(servidor, puerto);
                LoginResult resultado = sesion.TryConnectAndLogin(
                    "Transistor",
                    nombreJugador,
                    ItemsHandlingFlags.AllItems,
                    tags: new[] { "NoText" }
                );

                if (!resultado.Successful)
                {
                    File.AppendAllText(rutaLog, "Error: No se pudo conectar al servidor.\n");
                    return;
                }

                File.AppendAllText(rutaLog, "¡CONECTADO A ARCHIPELAGO!\n");

                while (true)
                {
                    while (sesion.Items.AllItemsReceived.Count > itemsEntregados)
                    {
                        ItemInfo itemNuevo = sesion.Items.AllItemsReceived[itemsEntregados];
                        itemsEntregados++;
                        lock (bloqueoCola)
                        {
                            colaItemsPendientes.Enqueue(itemNuevo.ItemName);
                        }
                    }

                    Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(rutaBase, "archipelago_error.txt"), ex.ToString());
            }
        }
    }

    // 1. Parche para createCard(string, CardType)
    [HarmonyPatch(typeof(Deck), "createCard", new Type[] { typeof(string), typeof(Card.CardType) })]
    public class ParcheDeckCard
    {
        static bool Prefix(string name, Card.CardType type, ref Card __result)
        {
            if (ModPrincipal.entregandoDesdeAP || name == "Jumper" || name == "ClusterbombGP")
                return true;

            ProcesarCartaInterceptada(name, type);
            __result = new Card(name, type, 0);
            return false;
        }

        public static void ProcesarCartaInterceptada(string name, Card.CardType type)
        {
            if (ModPrincipal.mapaChecksPorCarta.ContainsKey(name))
            {
                ModPrincipal.EnviarCheck(ModPrincipal.mapaChecksPorCarta[name], $"Funcion {name}");
            }
        }
    }

    // 2. Parche para createCard(string, CardType, int, int, int) (void)
    [HarmonyPatch(typeof(Deck), "createCard", new Type[] { typeof(string), typeof(Card.CardType), typeof(int), typeof(int), typeof(int) })]
    public class ParcheDeckVoid
    {
        static bool Prefix(string name, Card.CardType type)
        {
            if (ModPrincipal.entregandoDesdeAP || name == "Jumper" || name == "ClusterbombGP")
                return true;

            ParcheDeckCard.ProcesarCartaInterceptada(name, type);
            return false;
        }
    }

    // 3. Evitar que Deck.Buy regale cartas vanilla al confirmar
    [HarmonyPatch(typeof(Deck), "Buy", new Type[] { typeof(Card) })]
    public class ParcheDeckBuy
    {
        static bool Prefix(Deck __instance, Card card)
        {
            if (card != null && __instance.Store != null)
            {
                __instance.Store.Remove(card);
            }
            return false;
        }
    }

    // 4. Bloqueo de la Motocicleta hasta tener Motorcycle Key
    [HarmonyPatch(typeof(MotorcycleInput), "handleInput")]
    public class ParcheMoto
    {
        static bool Prefix()
        {
            return ModPrincipal.tieneLlaveMoto;
        }
    }

    // 5. Bloqueo del Bote de los Canales hasta tener Canal Boat Key
    [HarmonyPatch(typeof(BoatInput), "handleInput")]
    public class ParcheBote
    {
        static bool Prefix()
        {
            return ModPrincipal.tieneLlaveBote;
        }
    }

    // 6. Control de transiciones de mapa: Bloqueo de Glider/Ferry + Detección de los 4 Jefes y Victoria
    [HarmonyPatch]
    public class ParcheTransicionMapas
    {
        static MethodBase TargetMethod()
        {
            foreach (Type t in typeof(Deck).Assembly.GetTypes())
            {
                if (t.Name == "World")
                {
                    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        if (m.Name == "RequestBegin" && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType == typeof(string))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }

        static bool Prefix(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return true;

            string mapa = Path.GetFileNameWithoutExtension(mapName);
            File.AppendAllText(ModPrincipal.rutaLog, $"[MAPA] Intentando cargar mapa: {mapa}\n");

            // Si intenta tomar el planeador/ferry hacia Fairview sin tener Skyway Ferry Ticket, bloqueamos la transición
            if ((mapa.Equals("Glider01", StringComparison.OrdinalIgnoreCase) ||
                 mapa.Equals("Fairview01", StringComparison.OrdinalIgnoreCase)) &&
                !ModPrincipal.tieneTicketFerry)
            {
                File.AppendAllText(ModPrincipal.rutaLog, "[BLOQUEO] Viaje a Fairview/Glider bloqueado: falta Skyway Ferry Ticket.\n");
                return false;
            }

            return true;
        }

        static void Postfix(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return;
            string mapa = Path.GetFileNameWithoutExtension(mapName);

            // Jefe 1: Sybil Reisz (al completar Stage02 y pasar a Glider01 / Fairview01)
            if (mapa.Equals("Glider01", StringComparison.OrdinalIgnoreCase) ||
                mapa.Equals("Fairview01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78101, "Boss - Sybil Reisz");
            }
            // Jefe 2: The Spine (al completar Monster01 y pasar a Boat01 / Canals01)
            else if (mapa.Equals("Boat01", StringComparison.OrdinalIgnoreCase) ||
                     mapa.Equals("Canals01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78102, "Boss - The Spine");
            }
            // Jefe 3: Asher & Grant (al llegar a Showdown01 tras Bracket Towers)
            else if (mapa.Equals("Showdown01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78103, "Boss - Asher & Grant");
            }
            // Jefe 4: Royce Bracket + Victoria (al completar Showdown01 y pasar a Farewell01)
            else if (mapa.Equals("Farewell01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78104, "Boss - Royce Bracket");
                ModPrincipal.EnviarVictoria("Royce Bracket derrotado");
            }
        }
    }

    // 7. Detectar subida de nivel y enviar los checks de Level Up (2..15)
    [HarmonyPatch]
    public class ParcheSubidaNivel
    {
        static MethodBase TargetMethod()
        {
            foreach (Type t in typeof(Deck).Assembly.GetTypes())
            {
                if (t.Name == "ProgressHelper")
                {
                    var m = t.GetMethod("levelUp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (m != null) return m;
                }
            }
            return null;
        }

        static void Postfix()
        {
            ModPrincipal.nivelJugadorActual++;
            int lvl = ModPrincipal.nivelJugadorActual;
            File.AppendAllText(ModPrincipal.rutaLog, $"[NIVEL] ¡Subida a Nivel {lvl} detectada!\n");

            if (lvl >= 2 && lvl <= 15)
            {
                ModPrincipal.EnviarCheck(78200 + lvl, $"Level Up {lvl} - Option A");
                ModPrincipal.EnviarCheck(78300 + lvl, $"Level Up {lvl} - Option B");
            }

            if (lvl >= 25)
            {
                ModPrincipal.EnviarVictoria("Nivel 25 alcanzado");
            }
        }
    }

    // 8. Proteger BuyCard() por si en algún nivel avanzado Store queda vacío
    [HarmonyPatch(typeof(CardBuyScreen), "BuyCard")]
    public class ParcheCardBuySafe
    {
        static bool Prefix(CardBuyScreen __instance)
        {
            try
            {
                if (__instance.CardPane == null || __instance.CardPane.getSelected() == null)
                {
                    __instance.exit();
                    return false;
                }
            }
            catch { }
            return true;
        }
    }

    // 9. Procesar ítems recibidos de Archipelago en el HILO PRINCIPAL del juego
    [HarmonyPatch(typeof(GameplayScreen), "Update")]
    public class ParcheHiloPrincipalUI
    {
        static void Postfix()
        {
            ModPrincipal.ProcesarColaEnHiloPrincipal();
        }
    }

    // 10. Guardia preventiva en WeaponUI.SetupWeaponImage
    [HarmonyPatch]
    public class ParcheGuardiaWeaponUI
    {
        static MethodBase TargetMethod()
        {
            foreach (Type t in typeof(CardBuyScreen).Assembly.GetTypes())
            {
                if (t.Name == "WeaponUI")
                {
                    var m = t.GetMethod("SetupWeaponImage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (m != null) return m;
                }
            }
            return null;
        }

        static bool Prefix(Card card)
        {
            return card != null;
        }
    }
}