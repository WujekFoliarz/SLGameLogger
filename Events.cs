using System.Numerics;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.Features;
using PlayerRoles;
using UnityEngine.Experimental.UIElements;
using Utf8Json.Internal.DoubleConversion;

namespace SLGameLogger
{
    public struct EventData
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public EventEnum Event { get; set; } = EventEnum.None;
        public int GiverId { get; set; } = 0;
        public int ReceiverId { get; set; } = 0;
        public RoleTypeId RoleType { get; set; } = 0;
        public string CustomData { get; set; } = "";

        public EventData()
        {

        }
    }

    public struct TickEvent
    {
        public long Tick { get; set; }
        public List<EventData> Data { get; set; }
    }

    public class Events
    {
        public void OnRoundStarted()
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            Plugin.Instance.SetNewRoundTime();
            Plugin.Instance.StartCollectingData();
        }

        public void OnRoundEnded(Exiled.Events.EventArgs.Server.RoundEndedEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            Plugin.Instance.StopCollectingData();
        }

        public void OnRestartingRound()
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            Plugin.Instance.StopCollectingData();
        }

        public void OnInteractingDoor(Exiled.Events.EventArgs.Player.InteractingDoorEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (!ev.CanInteract)
            {
                return;
            }

            if (!ev.IsAllowed)
            {
                return;
            }

            var doorPosition = new System.Numerics.Vector3(ev.Door.Position.x, ev.Door.Position.y, ev.Door.Position.z);

            EventData eventData = new()
            {
                Event = ev.Door.IsOpen ? EventEnum.DoorClosed : EventEnum.DoorOpened,
                GiverId = ev.Player.Id,
                Position = doorPosition,
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnDying(Exiled.Events.EventArgs.Player.DyingEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            EventData eventData = new();
            eventData.Event = EventEnum.PlayerDied;

            if (ev.Attacker != null)
            {
                eventData.GiverId = ev.Attacker.Id;
            }

            if (ev.Player != null)
            {
                eventData.ReceiverId = ev.Player.Id;
            }

            if (ev.DamageHandler != null)
            {
                eventData.CustomData = ev.DamageHandler.Type.ToString();
            }

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnAnnouncingNtfEntrance(Exiled.Events.EventArgs.Map.AnnouncingNtfEntranceEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = ev.Wave.IsMiniWave ? EventEnum.NtfMiniWave : EventEnum.NtfWave,
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnAnnouncingChaosEntrance(Exiled.Events.EventArgs.Map.AnnouncingChaosEntranceEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = ev.Wave.IsMiniWave ? EventEnum.CIMiniWave : EventEnum.CIWave
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnPickingUpItem(Exiled.Events.EventArgs.Player.PickingUpItemEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Pickup == null)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = EventEnum.PickingUpItem,
                CustomData = ev.Pickup.Info.ItemId.ToString(),
                ReceiverId = ev.Player.Id,
                Position = new Vector3(ev.Player.Position.x, ev.Player.Position.y, ev.Player.Position.z)
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnThrownProjectile(Exiled.Events.EventArgs.Player.ThrownProjectileEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            EventEnum eventEnum = EventEnum.None;

            if (ev.Item.Type == ItemType.SCP018)
            {
                eventEnum = EventEnum.BallThrown;
            }
            else if (ev.Item.Type == ItemType.GrenadeHE)
            {
                eventEnum = EventEnum.GrenadeThrown;
            }
            else if (ev.Item.Type == ItemType.GrenadeFlash)
            {
                eventEnum = EventEnum.FlashGrenadeThrown;
            }

            if (eventEnum == EventEnum.None)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = eventEnum,
                GiverId = ev.Player.Id
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnExplodingGrenade(Exiled.Events.EventArgs.Map.ExplodingGrenadeEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            EventEnum eventEnum = EventEnum.None;

            if (ev.Projectile.Type == ItemType.GrenadeHE)
            {
                eventEnum = EventEnum.GrenadeExploded;
            }
            else if (ev.Projectile.Type == ItemType.GrenadeFlash)
            {
                eventEnum = EventEnum.FlashGrenadeExploded;
            }

            if (eventEnum == EventEnum.None)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = eventEnum,
                GiverId = ev.Player != null ? ev.Player.Id : 0,
                Position = new Vector3(ev.Projectile.Position.x, ev.Projectile.Position.y, ev.Projectile.Position.z)
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnHurting(Exiled.Events.EventArgs.Player.HurtingEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Player == null)
            {
                return;
            }

            EventEnum eventEnum = EventEnum.None;

            if (ev.DamageHandler != null && ev.DamageHandler.Type == Exiled.API.Enums.DamageType.Scp018)
            {
                eventEnum = EventEnum.HitByBall;
            }

            if (eventEnum == EventEnum.None)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = eventEnum,
                GiverId = ev.Attacker != null ? ev.Attacker.Id : 0,
                ReceiverId = ev.Player != null ? ev.Player.Id : 0
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnChangingMicroHIDState(Exiled.Events.EventArgs.Player.ChangingMicroHIDStateEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Player == null)
            {
                return;
            }

            EventEnum eventEnum = EventEnum.None;

            if (ev.NewPhase == InventorySystem.Items.MicroHID.Modules.MicroHidPhase.WindingUp)
            {
                eventEnum = EventEnum.ChargingMicroHid;
            }
            else if (ev.NewPhase == InventorySystem.Items.MicroHID.Modules.MicroHidPhase.WindingDown)
            {
                eventEnum = EventEnum.CancelChargingMicroHid;
            }
            else if (ev.NewPhase == InventorySystem.Items.MicroHID.Modules.MicroHidPhase.Firing)
            {
                eventEnum = EventEnum.FiringMicroHid;
            }

            if (eventEnum == EventEnum.None)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = eventEnum,
                GiverId = ev.Player != null ? ev.Player.Id : 0
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnEscaping(Exiled.Events.EventArgs.Player.EscapingEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Player == null)
            {
                return;
            }

            EventEnum eventEnum = EventEnum.None;

            if (ev.Player.IsCuffed)
            {
                eventEnum = EventEnum.PlayerEscorted;
            }
            else
            {
                eventEnum = EventEnum.PlayerEscaped;
            }

            if (eventEnum == EventEnum.None)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = eventEnum,
                GiverId = ev.Player.IsCuffed && ev.Player.Cuffer != null ? ev.Player.Cuffer.Id : 0,
                ReceiverId = ev.Player != null ? ev.Player.Id : 0
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnUsedItem(Exiled.Events.EventArgs.Player.UsedItemEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Player == null)
            {
                return;
            }

            EventEnum eventEnum = EventEnum.None;

            switch (ev.Item.Type)
            {
                case ItemType.SCP268:
                    eventEnum = EventEnum.SCP268Used;
                    break;
            }

            if (eventEnum == EventEnum.None)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = eventEnum,
                GiverId = ev.Player != null ? ev.Player.Id : 0
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnChangingWearables(Exiled.Events.EventArgs.Player.ChangingWearablesEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Player == null)
            {
                return;
            }

            EventEnum eventEnum = EventEnum.None;

            bool wears268 = ev.Player.Wearables.HasFlag(Exiled.API.Enums.WearableElementType.Scp268Hat);
            bool newWearablesHas268 = ev.NewWearables.HasFlag(Exiled.API.Enums.WearableElementType.Scp268Hat);
            if (wears268 && !newWearablesHas268)
            {
                eventEnum = EventEnum.SCP268Expired;
            }

            if (eventEnum == EventEnum.None)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = eventEnum,
                GiverId = ev.Player != null ? ev.Player.Id : 0
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Player == null)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = EventEnum.PlayerJoined,
                GiverId = ev.Player != null ? ev.Player.Id : 0,
                CustomData = ev.Player != null ? ev.Player.Nickname : "",
                RoleType = ev.Player != null ? ev.Player.Role.Type : RoleTypeId.None
            };

            Plugin.Instance.LogEvent(eventData);
        }

        public void OnChangingRole(Exiled.Events.EventArgs.Player.ChangingRoleEventArgs ev)
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            if (ev.Player == null)
            {
                return;
            }

            EventData eventData = new()
            {
                Event = EventEnum.PlayerChangedRoles,
                RoleType = ev.NewRole,
                ReceiverId = ev.Player.Id
            };

            Plugin.Instance.LogEvent(eventData);
        }
    }
}