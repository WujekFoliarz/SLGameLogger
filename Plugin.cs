namespace SLGameLogger
{
    using System.Diagnostics.Tracing;
    using System.Numerics;
    using CommandSystem.Commands.Shared;
    using Exiled.API.Features;
    using Exiled.Events.Features;
    using MEC;
    using Newtonsoft.Json;

    public class Plugin : Plugin<Config>
    {
        public static Plugin? Instance { get; private set; }
        private List<TickEvent>? _currentRoundEventLog;
        private List<EventData>? _queuedEventData;
        private CoroutineHandle _writerHandle;
        private Events? _events { get; set; }
        private static string _directory = Path.Combine(Paths.Plugins, "SLGameLogger", "Logs");
        private static DateTime _currentRoundStartTime = DateTime.UtcNow;
        private PacketWriter? _writer;

        public override void OnEnabled()
        {
            Instance = this;
            _events = new();
            _currentRoundEventLog = new();
            _queuedEventData = new();
            Directory.CreateDirectory(_directory);

            Exiled.Events.Handlers.Server.RoundStarted += _events.OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += _events.OnRoundEnded;
            Exiled.Events.Handlers.Server.RestartingRound += _events.OnRestartingRound;
            Exiled.Events.Handlers.Player.InteractingDoor += _events.OnInteractingDoor;
            Exiled.Events.Handlers.Player.Dying += _events.OnDying;
            Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += _events.OnAnnouncingNtfEntrance;
            Exiled.Events.Handlers.Map.AnnouncingChaosEntrance += _events.OnAnnouncingChaosEntrance;
            Exiled.Events.Handlers.Player.PickingUpItem += _events.OnPickingUpItem;
            Exiled.Events.Handlers.Player.ThrownProjectile += _events.OnThrownProjectile;
            Exiled.Events.Handlers.Map.ExplodingGrenade += _events.OnExplodingGrenade;
            Exiled.Events.Handlers.Player.Hurting += _events.OnHurting;
            Exiled.Events.Handlers.Player.ChangingMicroHIDState += _events.OnChangingMicroHIDState;
            Exiled.Events.Handlers.Player.Escaping += _events.OnEscaping;
            Exiled.Events.Handlers.Player.UsedItem += _events.OnUsedItem;
            Exiled.Events.Handlers.Player.ChangingWearables += _events.OnChangingWearables;
            Exiled.Events.Handlers.Player.Verified += _events.OnVerified;
            Exiled.Events.Handlers.Player.ChangingRole += _events.OnChangingRole;
        }

        public override void OnDisabled()
        {
            if (_events != null)
            {
                Exiled.Events.Handlers.Server.RoundStarted -= _events.OnRoundStarted;
                Exiled.Events.Handlers.Server.RoundEnded -= _events.OnRoundEnded;
                Exiled.Events.Handlers.Server.RestartingRound -= _events.OnRestartingRound;
                Exiled.Events.Handlers.Player.InteractingDoor -= _events.OnInteractingDoor;
                Exiled.Events.Handlers.Player.Dying -= _events.OnDying;
                Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= _events.OnAnnouncingNtfEntrance;
                Exiled.Events.Handlers.Map.AnnouncingChaosEntrance -= _events.OnAnnouncingChaosEntrance;
                Exiled.Events.Handlers.Player.PickingUpItem -= _events.OnPickingUpItem;
                Exiled.Events.Handlers.Player.ThrownProjectile -= _events.OnThrownProjectile;
                Exiled.Events.Handlers.Map.ExplodingGrenade -= _events.OnExplodingGrenade;
                Exiled.Events.Handlers.Player.Hurting -= _events.OnHurting;
                Exiled.Events.Handlers.Player.ChangingMicroHIDState -= _events.OnChangingMicroHIDState;
                Exiled.Events.Handlers.Player.Escaping -= _events.OnEscaping;
                Exiled.Events.Handlers.Player.UsedItem -= _events.OnUsedItem;
                Exiled.Events.Handlers.Player.ChangingWearables -= _events.OnChangingWearables;
                Exiled.Events.Handlers.Player.Verified -= _events.OnVerified;
                Exiled.Events.Handlers.Player.ChangingRole -= _events.OnChangingRole;
            }

            Instance = null;
        }

        public void LogEvent(EventData eventData)
        {
            if (_queuedEventData == null)
            {
                return;
            }

            _queuedEventData.Add(eventData);
        }

        public void StartCollectingData()
        {
            Log.Info("Started data collection");
            _writerHandle = Timing.RunCoroutine(WriterCoroutine());

            foreach (var room in Room.List)
            {
                EventData roomEv = new()
                {
                    Event = EventEnum.Room,
                    Position = new Vector3(room.Position.x, room.Position.y, room.Position.z),
                    Rotation = new Quaternion(room.Rotation.x, room.Rotation.y, room.Rotation.z, room.Rotation.w),
                    CustomData = room.Type.ToString(),
                };

                LogEvent(roomEv);
            }
        }

        public void StopCollectingData()
        {
            Log.Info("Stopped data collection");
            Timing.KillCoroutines(_writerHandle);
            Flush();

            _queuedEventData?.Clear();
        }

        private void Flush()
        {
            if (_currentRoundEventLog == null)
            {
                Log.Error("[Flush] _currentRoundEventLog was null");
                return;
            }

            if (_writer == null)
            {
                Log.Error("[Flush] _writer was null");
                return;
            }

            if (_currentRoundEventLog.Count == 0)
                return;

            foreach (var evt in _currentRoundEventLog)
            {
                _writer.WriteNewTick(evt.Tick);

                foreach (var data in evt.Data)
                {
                    switch (data.Event)
                    {
                        case EventEnum.PlayerPosition:
                            _writer.WritePlayerPosition(data.GiverId, data.Position, data.Rotation);
                            break;
                        case EventEnum.DoorOpened:
                            _writer.WriteDoorOpened(data.GiverId, data.Position);
                            break;
                        case EventEnum.DoorClosed:
                            _writer.WriteDoorClosed(data.GiverId, data.Position);
                            break;
                        case EventEnum.PlayerJoined:
                            _writer.WritePlayerJoined(data.GiverId, data.CustomData, (byte)data.RoleType);
                            break;
                        case EventEnum.PlayerLeft:
                            _writer.WritePlayerLeft(data.GiverId);
                            break;
                        case EventEnum.Room:
                            _writer.WriteRoom(data.CustomData, data.Position, data.Rotation);
                            break;
                        case EventEnum.PlayerDied:
                            _writer.WritePlayerDied(data.GiverId, data.ReceiverId, data.CustomData);
                            break;
                        case EventEnum.NtfWave:
                            _writer.WriteNtfWave();
                            break;
                        case EventEnum.NtfMiniWave:
                            _writer.WriteNtfMiniWave();
                            break;
                        case EventEnum.CIWave:
                            _writer.WriteCIWave();
                            break;
                        case EventEnum.CIMiniWave:
                            _writer.WriteCIMiniWave();
                            break;
                        case EventEnum.BallThrown:
                            _writer.WriteBallThrown(data.GiverId);
                            break;
                        case EventEnum.HitByBall:
                            _writer.WriteHitByBall(data.GiverId, data.ReceiverId);
                            break;
                        case EventEnum.GrenadeThrown:
                            _writer.WriteGrenadeThrown(data.GiverId);
                            break;
                        case EventEnum.GrenadeExploded:
                            _writer.WriteGrenadeExploded(data.GiverId, data.Position);
                            break;
                        case EventEnum.PickingUpItem:
                            _writer.WritePickingUpItem(data.ReceiverId, data.CustomData, data.Position);
                            break;
                        case EventEnum.FlashGrenadeThrown:
                            _writer.WriteFlashGrenadeThrown(data.GiverId);
                            break;
                        case EventEnum.FlashGrenadeExploded:
                            _writer.WriteFlashGrenadeExploded(data.GiverId, data.Position);
                            break;
                        case EventEnum.ChargingMicroHid:
                            _writer.WriteChargingMicroHid(data.GiverId);
                            break;
                        case EventEnum.CancelChargingMicroHid:
                            _writer.WriteCancelChargingMicroHid(data.GiverId);
                            break;
                        case EventEnum.FiringMicroHid:
                            _writer.WriteFiringMicroHid(data.GiverId);
                            break;
                        case EventEnum.PlayerEscaped:
                            _writer.WritePlayerEscaped(data.ReceiverId);
                            break;
                        case EventEnum.PlayerEscorted:
                            _writer.WritePlayerEscorted(data.GiverId, data.ReceiverId);
                            break;
                        case EventEnum.SCP268Used:
                            _writer.WriteSCP268Used(data.GiverId);
                            break;
                        case EventEnum.SCP268Expired:
                            _writer.WriteSCP268Expired(data.GiverId);
                            break;
                        case EventEnum.PlayerChangedRoles:
                            _writer.WritePlayerChangedRoles(data.ReceiverId, (byte)data.RoleType);
                            break;
                        default:
                            Log.Error($"[SLGameLogger/Flush] {data.Event} unimplemented");
                            break;
                    }
                }
            }

            _writer.Flush();
            _currentRoundEventLog.Clear();
        }

        private List<EventData> GetPlayerPositionData()
        {
            List<EventData> data = new List<EventData>();

            foreach (var player in Player.List)
            {
                EventData eventData = new()
                {
                    Position = new Vector3(player.Position.x, player.Position.y, player.Position.z),
                    Rotation = new Quaternion(player.Rotation.x, player.Rotation.y, player.Rotation.z, player.Rotation.w),
                    Event = EventEnum.PlayerPosition,
                    GiverId = player != null ? player.Id : 0
                };
                data.Add(eventData);
            }

            return data;
        }

        private IEnumerator<float> WriterCoroutine()
        {
            string fileName = "RoundLog " + _currentRoundStartTime.ToString("yyyy-MM-dd_HH-mm-ss") + ".scpd";
            using var stream = File.Open(Path.Combine(_directory, fileName), FileMode.Create);
            _writer = new(stream);

            if (_currentRoundEventLog == null)
            {
                Log.Error("_currentRoundEventLog was null");
                yield break;
            }

            if (_queuedEventData == null)
            {
                Log.Error("_queuedEventData was null");
                yield break;
            }

            int currentTick = 0;

            while (true)
            {
                try
                {
                    TickEvent tickEvent = new();
                    tickEvent.Tick = currentTick;
                    tickEvent.Data = new();

                    foreach (var data in _queuedEventData)
                    {
                        tickEvent.Data.Add(data);
                    }
                    _queuedEventData.Clear();

                    if (currentTick % Config.SavePlayerPositionPerTicks == 0)
                    {
                        var playerPositions = GetPlayerPositionData();

                        foreach (var playerPosition in playerPositions)
                        {
                            tickEvent.Data.Add(playerPosition);
                        }
                    }

                    if (tickEvent.Data.Count > 0)
                    {
                        _currentRoundEventLog.Add(tickEvent);
                    }

                    currentTick++;

                    if (currentTick % Config.FlushDataPerTicks == 0)
                    {
                        Flush();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"WriterCoroutine exception:\n{ex}");
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        public void SetNewRoundTime()
        {
            _currentRoundStartTime = DateTime.UtcNow;
        }
    }
}