using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class InfoScoutAP
    {
        public string NombreItem;
        public string NombreJugador;
        public string CartaInterna;
    }

    public class ModPrincipal
    {
        private static bool yaIniciado = false;
        public static ArchipelagoSession sesion;
        public static int itemsEntregados = 0;
        public static bool entregandoDesdeAP = false;
        public static bool victoriaEnviada = false;
        public static int nivelJugadorActual = 1;

        // Opciones leídas desde slot_data de Archipelago
        // 0 = Royce Bracket, 1 = Sandbox (25 pruebas), 2 = Nivel Máximo (20)
        public static int metaActiva = 0;
        public static bool sandboxActivo = true;
        public static bool requiereLlaves = true;

        private static readonly Queue<string> colaItemsPendientes = new Queue<string>();
        private static readonly object bloqueoCola = new object();

        public static bool tieneLlaveMoto = false;
        public static bool tieneTicketFerry = false;
        public static bool tieneLlaveBote = false;
        public static int bonusMemTotal = 0;

        private static readonly List<string> habilidadesDesbloqueadas = new List<string>();
        private static readonly HashSet<string> cartasEntregadasEnDeckActual = new HashSet<string>();
        private static readonly HashSet<long> checksEnviados = new HashSet<long>();
        private static string ultimaCartaEquipada0 = "";
        private static string ultimaCartaEquipada1 = "";
        private static int framesEsperaInicio = 0;
        private static bool yaEquipoInicio = false;
        private static Deck ultimoDeckDetectado = null;
        private static readonly Random rngVisual = new Random();

        public static readonly Dictionary<Card, string> titulosAPStore = new Dictionary<Card, string>();
        public static readonly Dictionary<Card, string> descripcionesAPStore = new Dictionary<Card, string>();

        public static string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
        public static string rutaLog => Path.Combine(rutaBase, "archipelago_log.txt");

        public static readonly HashSet<string> cartasValidasSet = new HashSet<string>
        {
            "Slam", "Snipe", "Blink", "Bomb",
            "Orb", "Launch", "Charm", "Hide",
            "Sidearm", "Summon", "Uppercut", "Hook",
            "Tracker", "Heal", "Flusher", "Might"
        };

        public static readonly string[] todasCartasInternas = new string[]
        {
            "Slam", "Snipe", "Blink", "Bomb",
            "Orb", "Launch", "Charm", "Hide",
            "Sidearm", "Summon", "Uppercut", "Hook",
            "Tracker", "Heal", "Flusher", "Might"
        };

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

        public static string ObtenerCartaVisualAleatoria(string excluir1, string excluir2)
        {
            for (int intento = 0; intento < 30; intento++)
            {
                string candidata = todasCartasInternas[rngVisual.Next(todasCartasInternas.Length)];
                if (candidata != excluir1 && candidata != excluir2 && !habilidadesDesbloqueadas.Contains(candidata))
                    return candidata;
            }
            return excluir1 == "Slam" ? "Snipe" : "Slam";
        }

        private static string ObtenerNombreDestinatario(object scoutInfo)
        {
            if (scoutInfo == null) return "Archipelago";
            try
            {
                PropertyInfo propPlayer = scoutInfo.GetType().GetProperty("Player");
                if (propPlayer != null)
                {
                    object valPlayer = propPlayer.GetValue(scoutInfo, null);
                    if (valPlayer is int slotId && sesion != null)
                    {
                        string alias = sesion.Players.GetPlayerAlias(slotId);
                        if (!string.IsNullOrEmpty(alias)) return alias;
                    }
                    else if (valPlayer != null)
                    {
                        PropertyInfo propName = valPlayer.GetType().GetProperty("Name") ?? valPlayer.GetType().GetProperty("Alias");
                        if (propName != null)
                        {
                            string nombre = propName.GetValue(valPlayer, null) as string;
                            if (!string.IsNullOrEmpty(nombre)) return nombre;
                        }
                    }
                }
            }
            catch { }
            return "Archipelago";
        }

        public static List<InfoScoutAP> ConsultarInfoDeNivel(int nivel)
        {
            List<InfoScoutAP> lista = new List<InfoScoutAP>();
            try
            {
                if (sesion != null && sesion.Socket.Connected && nivel >= 2 && nivel <= 15)
                {
                    long idA = 78200 + nivel;
                    long idB = 78300 + nivel;
                    var tarea = sesion.Locations.ScoutLocationsAsync(false, idA, idB);
                    if (tarea.Wait(450) && tarea.Result != null)
                    {
                        long[] ordenIds = new long[] { idA, idB };
                        foreach (long id in ordenIds)
                        {
                            if (tarea.Result.ContainsKey(id) && tarea.Result[id] != null)
                            {
                                var scout = tarea.Result[id];
                                string nombreItem = scout.ItemName ?? "Objeto AP";
                                string jugadorDest = ObtenerNombreDestinatario(scout);
                                string interna = mapaHabilidades.ContainsKey(nombreItem) ? mapaHabilidades[nombreItem] : "";

                                lista.Add(new InfoScoutAP
                                {
                                    NombreItem = nombreItem,
                                    NombreJugador = jugadorDest,
                                    CartaInterna = interna
                                });
                            }
                        }
                    }
                }
            }
            catch { }
            return lista;
        }

        public static int ObtenerSiguienteNivelPendiente()
        {
            try
            {
                if (sesion != null && sesion.Socket.Connected)
                {
                    var completados = sesion.Locations.AllLocationsChecked;
                    for (int lvl = 2; lvl <= 15; lvl++)
                    {
                        if (!completados.Contains(78200 + lvl) || !completados.Contains(78300 + lvl))
                        {
                            return lvl;
                        }
                    }
                }
            }
            catch { }
            return nivelJugadorActual + 1;
        }

        public static void VerificarVictoriaSandbox()
        {
            if (metaActiva != 1 || victoriaEnviada || sesion == null || !sesion.Socket.Connected)
                return;

            try
            {
                var completados = sesion.Locations.AllLocationsChecked;
                for (long id = 78401; id <= 78425; id++)
                {
                    if (!completados.Contains(id) && !checksEnviados.Contains(id))
                        return;
                }
                EnviarVictoria("Todas las 25 pruebas del Sandbox completadas");
            }
            catch { }
        }

        public static void RegistrarTextoAPEnCarta(Card carta, InfoScoutAP info)
        {
            if (carta == null || info == null) return;
            titulosAPStore[carta] = $"[AP] {info.NombreItem} ({info.NombreJugador})";
            descripcionesAPStore[carta] = $"Archipelago: recibirás/enviarás '{info.NombreItem}' para {info.NombreJugador}. Al confirmar, se envían ambos checks de este nivel.";
        }

        public static void Iniciar()
        {
            if (yaIniciado) return;
            yaIniciado = true;

            File.WriteAllText(rutaLog, "Transistor Archipelago v3.39 iniciado.\n");

            Harmony harmony = new Harmony("com.transistor.archipelago");
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0)
                {
                    try
                    {
                        harmony.CreateClassProcessor(t).Patch();
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(rutaLog, $"[WARN PATCH] {t.Name}: {ex.Message}\n");
                    }
                }
            }

            Thread hiloMod = new Thread(BuclePrincipal);
            hiloMod.IsBackground = true;
            hiloMod.Start();
        }

        public static bool EsLlamadaPermitidaSinInterceptar()
        {
            if (ultimoDeckDetectado == null) return true;

            try
            {
                var st = new StackTrace();
                for (int i = 0; i < st.FrameCount; i++)
                {
                    var m = st.GetFrame(i)?.GetMethod();
                    if (m?.DeclaringType != null)
                    {
                        string tipo = m.DeclaringType.Name;
                        string metodo = m.Name;
                        if (tipo == "CardBuyScreen" ||
                            tipo == "HandScreen" ||
                            tipo == "TerminalScreen" ||
                            tipo == "Player" ||
                            tipo.Contains("Save") ||
                            tipo.Contains("Load") ||
                            tipo.Contains("Profile") ||
                            metodo.Contains("Load") ||
                            metodo.Contains("Read") ||
                            metodo.Contains("Deserialize"))
                            return true;
                    }
                }
            }
            catch { }
            return false;
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
                    if (idCheck >= 78401 && idCheck <= 78425)
                    {
                        VerificarVictoriaSandbox();
                    }
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

        private static void ExtraerNombresDeObjeto(object obj, HashSet<string> destino, int profundidad = 0)
        {
            if (obj == null || profundidad > 2) return;

            if (obj is Card c)
            {
                if (!string.IsNullOrEmpty(c.Name)) destino.Add(c.Name);
                return;
            }

            if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
            {
                foreach (object item in enumerable)
                {
                    ExtraerNombresDeObjeto(item, destino, profundidad + 1);
                }
                return;
            }

            Type t = obj.GetType();
            if (t.Name.Contains("Slot") || t.Name.Contains("Card"))
            {
                foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (p.PropertyType == typeof(Card) && p.GetIndexParameters().Length == 0)
                    {
                        try
                        {
                            Card inner = p.GetValue(obj, null) as Card;
                            if (inner != null && !string.IsNullOrEmpty(inner.Name))
                                destino.Add(inner.Name);
                        }
                        catch { }
                    }
                }
                foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (f.FieldType == typeof(Card))
                    {
                        try
                        {
                            Card inner = f.GetValue(obj) as Card;
                            if (inner != null && !string.IsNullOrEmpty(inner.Name))
                                destino.Add(inner.Name);
                        }
                        catch { }
                    }
                }
            }
        }

        public static bool MazoYaTieneCarta(Deck deck, string nombreInterno)
        {
            if (deck == null) return false;
            try
            {
                HashSet<string> todas = new HashSet<string>();
                ExtraerNombresDeObjeto(deck.Library, todas);
                ExtraerNombresDeObjeto(deck.Hand, todas);

                foreach (FieldInfo f in typeof(Deck).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (f.Name == "Store") continue;
                    ExtraerNombresDeObjeto(f.GetValue(deck), todas);
                }
                return todas.Contains(nombreInterno);
            }
            catch { }
            return false;
        }

        public static void LimpiarDuplicadosEnLibrary(Deck deck)
        {
            if (deck == null) return;
            try
            {
                if (deck.Hand != null)
                {
                    for (int i = deck.Hand.Count - 1; i >= 0; i--)
                    {
                        Card c = deck.Hand[i];
                        if (c == null || string.IsNullOrEmpty(c.Name) || !cartasValidasSet.Contains(c.Name))
                        {
                            deck.Hand.RemoveAt(i);
                        }
                    }
                }

                if (deck.Library == null) return;

                HashSet<string> vistasEnLibrary = new HashSet<string>();
                for (int i = deck.Library.Count - 1; i >= 0; i--)
                {
                    Card c = deck.Library[i];
                    if (c == null || string.IsNullOrEmpty(c.Name) || !cartasValidasSet.Contains(c.Name))
                    {
                        deck.Library.RemoveAt(i);
                        continue;
                    }

                    if (vistasEnLibrary.Contains(c.Name))
                    {
                        deck.Library.RemoveAt(i);
                    }
                    else
                    {
                        vistasEnLibrary.Add(c.Name);
                    }
                }
            }
            catch { }
        }

        private static Card ObtenerCartaDeMazo(Deck deck, string nombreObjetivo)
        {
            if (deck == null) return null;
            if (deck.Library != null)
            {
                for (int i = 0; i < deck.Library.Count; i++)
                {
                    Card c = deck.Library[i];
                    if (c != null && c.Name == nombreObjetivo)
                        return c;
                }
            }
            if (deck.Hand != null)
            {
                for (int i = 0; i < deck.Hand.Count; i++)
                {
                    Card c = deck.Hand[i];
                    if (c != null && c.Name == nombreObjetivo)
                        return c;
                }
            }
            return null;
        }

        private static void EquiparCartaEnBarra(Deck deck, Card carta, int slotDestino)
        {
            if (deck == null || carta == null) return;
            try
            {
                if (slotDestino == 0 && ultimaCartaEquipada0 != "") return;
                if (slotDestino == 1 && ultimaCartaEquipada1 != "") return;
                if (carta.Name == ultimaCartaEquipada0 || carta.Name == ultimaCartaEquipada1) return;

                deck.AdjustMaxActiveSize(6);
                deck.DrawCard(carta);
                deck.playCard(carta, slotDestino, true);

                if (slotDestino == 0) ultimaCartaEquipada0 = carta.Name;
                if (slotDestino == 1) ultimaCartaEquipada1 = carta.Name;
                File.AppendAllText(rutaLog, $"[EQUIPADO] '{carta.Name}' en Active[{slotDestino}] (Activos={deck.CalcNumActivePowers()}).\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(rutaLog, $"[ERROR EQUIPAR] {ex.Message}\n");
            }
        }

        public static void ProcesarColaEnHiloPrincipal()
        {
            try
            {
                Player jugador = PlayerManager.getPlayer();
                if (jugador == null || jugador.Deck == null) return;

                if (!ReferenceEquals(jugador.Deck, ultimoDeckDetectado))
                {
                    ultimoDeckDetectado = jugador.Deck;
                    cartasEntregadasEnDeckActual.Clear();
                    ultimaCartaEquipada0 = "";
                    ultimaCartaEquipada1 = "";
                    framesEsperaInicio = 0;
                    yaEquipoInicio = false;
                    LimpiarDuplicadosEnLibrary(jugador.Deck);
                    File.AppendAllText(rutaLog, "[REINICIO] Nuevo mazo detectado: restaurando habilidades desbloqueadas.\n");
                }

                lock (bloqueoCola)
                {
                    while (colaItemsPendientes.Count > 0)
                    {
                        string nombreAP = colaItemsPendientes.Dequeue();

                        if (mapaHabilidades.ContainsKey(nombreAP))
                        {
                            string nombreInterno = mapaHabilidades[nombreAP];
                            if (!habilidadesDesbloqueadas.Contains(nombreInterno))
                                habilidadesDesbloqueadas.Add(nombreInterno);

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
                            bonusMemTotal += 2;
                            jugador.Deck.AdjustMaxActiveSize(2);
                            File.AppendAllText(rutaLog, "[RECIBIDO] +2 MEM aplicado al mazo.\n");
                        }
                        else if (nombreAP == "Passive Slot Unlock")
                        {
                            File.AppendAllText(rutaLog, "[RECIBIDO] Ranura Pasiva desbloqueada.\n");
                        }
                        else if (nombreAP == "Data Fragment")
                        {
                            File.AppendAllText(rutaLog, "[RECIBIDO] Fragmento de datos (relleno).\n");
                        }
                    }
                }

                for (int i = 0; i < habilidadesDesbloqueadas.Count; i++)
                {
                    string nombreInterno = habilidadesDesbloqueadas[i];
                    if (cartasEntregadasEnDeckActual.Contains(nombreInterno)) continue;

                    cartasEntregadasEnDeckActual.Add(nombreInterno);
                    if (!MazoYaTieneCarta(jugador.Deck, nombreInterno))
                    {
                        entregandoDesdeAP = true;
                        jugador.Deck.createCard(nombreInterno, Card.CardType.WEAPON);
                        entregandoDesdeAP = false;
                    }
                }

                if (!yaEquipoInicio &&
                    checksEnviados.Contains(78001) &&
                    checksEnviados.Contains(78002) &&
                    habilidadesDesbloqueadas.Count >= 2)
                {
                    framesEsperaInicio++;
                    if (framesEsperaInicio >= 60)
                    {
                        yaEquipoInicio = true;

                        Card c0 = ObtenerCartaDeMazo(jugador.Deck, habilidadesDesbloqueadas[0]);
                        Card c1 = ObtenerCartaDeMazo(jugador.Deck, habilidadesDesbloqueadas[1]);

                        if (c0 != null) EquiparCartaEnBarra(jugador.Deck, c0, 0);
                        if (c1 != null) EquiparCartaEnBarra(jugador.Deck, c1, 1);

                        LimpiarDuplicadosEnLibrary(jugador.Deck);
                    }
                }
            }
            catch (Exception ex)
            {
                entregandoDesdeAP = false;
                File.AppendAllText(rutaLog, $"[ERROR PROCESAR COLA] {ex.Message}\n");
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

                // Leer slot_data (goal, include_sandbox, require_keys)
                if (resultado is LoginSuccessful loginOk && loginOk.SlotData != null)
                {
                    if (loginOk.SlotData.TryGetValue("goal", out object goalObj) && goalObj != null)
                    {
                        int.TryParse(goalObj.ToString(), out metaActiva);
                    }
                    if (loginOk.SlotData.TryGetValue("include_sandbox", out object sbObj) && sbObj != null)
                    {
                        bool.TryParse(sbObj.ToString(), out sandboxActivo);
                    }
                    if (loginOk.SlotData.TryGetValue("require_keys", out object keysObj) && keysObj != null)
                    {
                        bool.TryParse(keysObj.ToString(), out requiereLlaves);
                        if (!requiereLlaves)
                        {
                            tieneLlaveMoto = true;
                            tieneTicketFerry = true;
                            tieneLlaveBote = true;
                        }
                    }
                }

                string nombreMeta = metaActiva == 1 ? "Sandbox (25 pruebas)" : (metaActiva == 2 ? "Nivel 20" : "Royce Bracket");
                File.AppendAllText(rutaLog, $"¡CONECTADO A ARCHIPELAGO! Meta: {nombreMeta} | Sandbox: {sandboxActivo} | Llaves: {requiereLlaves}\n");

                while (true)
                {
                    lock (bloqueoCola)
                    {
                        while (sesion.Items.AllItemsReceived.Count > itemsEntregados)
                        {
                            ItemInfo itemNuevo = sesion.Items.AllItemsReceived[itemsEntregados];
                            itemsEntregados++;
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
        static bool Prefix(Deck __instance, string name, Card.CardType type, ref Card __result)
        {
            if (ModPrincipal.entregandoDesdeAP || name == "Jumper" || name == "ClusterbombGP")
                return true;

            if (ModPrincipal.EsLlamadaPermitidaSinInterceptar())
            {
                if (__instance != null && __instance.Library != null)
                {
                    for (int i = 0; i < __instance.Library.Count; i++)
                    {
                        if (__instance.Library[i]?.Name == name)
                        {
                            __result = __instance.Library[i];
                            return false;
                        }
                    }
                }
                return true;
            }

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
        static bool Prefix(Deck __instance, string name, Card.CardType type)
        {
            if (ModPrincipal.entregandoDesdeAP || name == "Jumper" || name == "ClusterbombGP")
                return true;

            if (ModPrincipal.EsLlamadaPermitidaSinInterceptar())
            {
                if (ModPrincipal.MazoYaTieneCarta(__instance, name))
                    return false;
                return true;
            }

            ParcheDeckCard.ProcesarCartaInterceptada(name, type);
            return false;
        }
    }

    // 3. Permitir mejoras de personaje (MEM, pasivas, puertas traseras) y solo bloquear regalos de armas vanilla
    [HarmonyPatch(typeof(Deck), "Buy", new Type[] { typeof(Card) })]
    public class ParcheDeckBuy
    {
        static bool Prefix(Deck __instance, Card card)
        {
            if (card != null && card.Type == Card.CardType.WEAPON)
            {
                if (__instance.Store != null)
                    __instance.Store.Remove(card);
                return false;
            }
            return true;
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

        static bool Prefix(string fullMapName)
        {
            if (string.IsNullOrEmpty(fullMapName)) return true;

            string mapa = Path.GetFileNameWithoutExtension(fullMapName);
            File.AppendAllText(ModPrincipal.rutaLog, $"[MAPA] Intentando cargar mapa: {mapa}\n");

            if ((mapa.Equals("Glider01", StringComparison.OrdinalIgnoreCase) ||
                 mapa.Equals("Fairview01", StringComparison.OrdinalIgnoreCase)) &&
                !ModPrincipal.tieneTicketFerry)
            {
                File.AppendAllText(ModPrincipal.rutaLog, "[BLOQUEO] Viaje a Fairview/Glider bloqueado: falta Skyway Ferry Ticket.\n");
                return false;
            }

            return true;
        }

        static void Postfix(string fullMapName)
        {
            if (string.IsNullOrEmpty(fullMapName)) return;
            string mapa = Path.GetFileNameWithoutExtension(fullMapName);

            if (mapa.Equals("Boat01", StringComparison.OrdinalIgnoreCase) ||
                mapa.Equals("Canals01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78101, "Boss - Sybil Reisz");
            }
            else if (mapa.Equals("Rooftops01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78102, "Boss - The Spine");
            }
            else if (mapa.Equals("Showdown01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78103, "Boss - Asher & Grant");
            }
            else if (mapa.Equals("Farewell01", StringComparison.OrdinalIgnoreCase))
            {
                ModPrincipal.EnviarCheck(78104, "Boss - Royce Bracket");
                if (ModPrincipal.metaActiva == 0)
                {
                    ModPrincipal.EnviarVictoria("Royce Bracket derrotado");
                }
            }
        }
    }

    // 7. Pantalla de subida de nivel: sincroniza el nivel real con Archipelago y envía los checks pendientes
    [HarmonyPatch]
    public class ParcheSubidaNivelPantalla
    {
        static MethodBase TargetMethod()
        {
            var ctors = typeof(CardBuyScreen).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return ctors.Length > 0 ? ctors[0] : null;
        }

        static void Prefix()
        {
            try
            {
                Player p = PlayerManager.getPlayer();
                if (p?.Deck?.Store == null) return;

                ModPrincipal.titulosAPStore.Clear();
                ModPrincipal.descripcionesAPStore.Clear();

                int proximoNivel = ModPrincipal.ObtenerSiguienteNivelPendiente();
                List<InfoScoutAP> infosAP = ModPrincipal.ConsultarInfoDeNivel(proximoNivel);

                bool tieneMejoraNoArma = false;
                for (int i = 0; i < p.Deck.Store.Count; i++)
                {
                    if (p.Deck.Store[i] != null && p.Deck.Store[i].Type != Card.CardType.WEAPON)
                    {
                        tieneMejoraNoArma = true;
                        break;
                    }
                }

                if (tieneMejoraNoArma)
                {
                    for (int i = 0; i < p.Deck.Store.Count && i < infosAP.Count; i++)
                    {
                        ModPrincipal.RegistrarTextoAPEnCarta(p.Deck.Store[i], infosAP[i]);
                    }
                    return;
                }

                p.Deck.Store.Clear();

                string carta1 = (infosAP.Count >= 1 && !string.IsNullOrEmpty(infosAP[0].CartaInterna))
                    ? infosAP[0].CartaInterna
                    : ModPrincipal.ObtenerCartaVisualAleatoria("", "");

                string carta2 = (infosAP.Count >= 2 && !string.IsNullOrEmpty(infosAP[1].CartaInterna))
                    ? infosAP[1].CartaInterna
                    : ModPrincipal.ObtenerCartaVisualAleatoria(carta1, "");

                Card obj1 = new Card(carta1, Card.CardType.WEAPON, 0);
                Card obj2 = new Card(carta2, Card.CardType.WEAPON, 0);

                if (infosAP.Count >= 1) ModPrincipal.RegistrarTextoAPEnCarta(obj1, infosAP[0]);
                if (infosAP.Count >= 2) ModPrincipal.RegistrarTextoAPEnCarta(obj2, infosAP[1]);

                p.Deck.Store.Add(obj1);
                p.Deck.Store.Add(obj2);
            }
            catch { }
        }

        static void Postfix()
        {
            int lvl = ModPrincipal.ObtenerSiguienteNivelPendiente();
            ModPrincipal.nivelJugadorActual = lvl;
            File.AppendAllText(ModPrincipal.rutaLog, $"[NIVEL] Pantalla de subida de nivel abierta -> Nivel {lvl}\n");

            if (lvl >= 2 && lvl <= 15)
            {
                ModPrincipal.EnviarCheck(78200 + lvl, $"Level Up {lvl} - Option A");
                ModPrincipal.EnviarCheck(78300 + lvl, $"Level Up {lvl} - Option B");
            }

            if (ModPrincipal.metaActiva == 2 && lvl >= 20)
            {
                ModPrincipal.EnviarVictoria("Nivel 20 alcanzado");
            }
        }
    }

    // 8. Mostrar título y descripción reales de Archipelago en cartas de la tienda de subida de nivel
    [HarmonyPatch]
    public class ParcheTextoCartaAP
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (MethodInfo m in typeof(Card).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (m.IsAbstract || m.IsGenericMethod || m.ReturnType != typeof(string) || m.GetParameters().Length != 0)
                    continue;

                string n = m.Name;
                if (n.Contains("Icon") || n.Contains("Texture") || n.Contains("File") || n == "ToString" || n == "get_Name")
                    continue;

                if (n.Contains("Name") || n.Contains("Title") || n.Contains("Desc") || n.Contains("Text") || n.Contains("Summary"))
                {
                    yield return m;
                }
            }
        }

        static void Postfix(Card __instance, MethodBase __originalMethod, ref string __result)
        {
            if (__instance == null || ModPrincipal.titulosAPStore.Count == 0) return;

            if (ModPrincipal.titulosAPStore.ContainsKey(__instance))
            {
                string n = __originalMethod.Name;
                if (n.Contains("Name") || n.Contains("Title"))
                {
                    __result = ModPrincipal.titulosAPStore[__instance];
                }
                else if (ModPrincipal.descripcionesAPStore.ContainsKey(__instance))
                {
                    __result = ModPrincipal.descripcionesAPStore[__instance];
                }
            }
        }
    }

    // 9. Permitir que mejoras (MEM/pasivas/puertas) se apliquen normalmente y cerrar seguro si es arma
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

    // 10. Procesar ítems recibidos de Archipelago en el HILO PRINCIPAL del juego
    [HarmonyPatch(typeof(GameplayScreen), "Update")]
    public class ParcheHiloPrincipalUI
    {
        static void Postfix()
        {
            ModPrincipal.ProcesarColaEnHiloPrincipal();
        }
    }

    // 11. Blindaje total en WeaponUI.SetupWeaponImage
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

        static bool Prefix(Card card, object weaponImage, object ammoText, object cooldownText, object cooldownAnim)
        {
            return card != null && weaponImage != null && ammoText != null && cooldownText != null && cooldownAnim != null;
        }

        static Exception Finalizer(Exception __exception)
        {
            return null;
        }
    }

    // 12. Limpiar copias duplicadas y cartas corruptas al abrir TerminalScreen o HandScreen
    [HarmonyPatch]
    public class ParcheTerminalLimpieza
    {
        static MethodBase TargetMethod()
        {
            foreach (Type t in typeof(CardBuyScreen).Assembly.GetTypes())
            {
                if (t.Name == "TerminalScreen")
                {
                    var ctors = t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (ctors.Length > 0) return ctors[0];
                }
            }
            return null;
        }

        static void Postfix()
        {
            try
            {
                Player p = PlayerManager.getPlayer();
                if (p?.Deck != null)
                {
                    ModPrincipal.LimpiarDuplicadosEnLibrary(p.Deck);
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(HandScreen), "setupHand")]
    public class ParcheHandScreenLimpieza
    {
        static void Prefix()
        {
            try
            {
                Player p = PlayerManager.getPlayer();
                if (p?.Deck != null)
                {
                    ModPrincipal.LimpiarDuplicadosEnLibrary(p.Deck);
                }
            }
            catch { }
        }
    }

    // 13. Evitar que HandScreen.InsertCard crashee con cartas nulas o corruptas en partidas guardadas
    [HarmonyPatch]
    public class ParcheHandInsertCardSafe
    {
        static MethodBase TargetMethod()
        {
            foreach (var m in typeof(HandScreen).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (m.Name == "InsertCard") return m;
            }
            return null;
        }

        static bool Prefix(Card card)
        {
            if (card == null || string.IsNullOrEmpty(card.Name) || !ModPrincipal.cartasValidasSet.Contains(card.Name))
                return false;
            return true;
        }

        static Exception Finalizer(Exception __exception)
        {
            return null;
        }
    }

    // 14. Blindaje contra cartas corruptas sin icono en partidas guardadas (Card.GetIconName)
    [HarmonyPatch(typeof(Card), "GetIconName")]
    public class ParcheCardIconSafe
    {
        static Exception Finalizer(Exception __exception, ref string __result)
        {
            if (__exception != null)
            {
                if (string.IsNullOrEmpty(__result))
                    __result = "SlamIcon";
                return null;
            }
            return null;
        }
    }

    // 15. Blindaje contra NullReferenceException en HandScreen.Update
    [HarmonyPatch(typeof(HandScreen), "Update")]
    public class ParcheHandScreenUpdateSafe
    {
        static Exception Finalizer(Exception __exception)
        {
            if (__exception is NullReferenceException)
                return null;
            return __exception;
        }
    }

    // 16. Blindaje contra NullReferenceException en Card.canUseSlot (firma corregida sin ref bool)
    [HarmonyPatch(typeof(Card), "canUseSlot")]
    public class ParcheCardCanUseSlotSafe
    {
        static Exception Finalizer(Exception __exception)
        {
            if (__exception is NullReferenceException)
                return null;
            return __exception;
        }
    }
}
