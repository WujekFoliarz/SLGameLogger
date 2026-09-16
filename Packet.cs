using System.Numerics;
using Exiled.API.Enums;

namespace SLGameLogger
{
    class PacketWriter
    {
        private readonly BinaryWriter _writer;
        private readonly MemoryStream _ms;

        public PacketWriter(Stream stream)
        {
            _writer = new BinaryWriter(stream);
            _ms = new MemoryStream();
        }

        private void AddPacket(EventEnum eventEnum, Action<BinaryWriter> writePayload)
        {
            _ms.SetLength(0);
            _ms.Position = 0;

            using (var payloadWriter = new BinaryWriter(_ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writePayload(payloadWriter);
            }

            byte[] payload = _ms.ToArray();

            _writer.Write((byte)eventEnum);
            _writer.Write((ushort)payload.Length);
            _writer.Write(payload);
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void WriteNewTick(long tick)
        {
            AddPacket(EventEnum.NewTick, w =>
            {
                w.Write(tick);
            });
        }

        public void WritePlayerPosition(int playerId, Vector3 position, Quaternion rotation)
        {
            AddPacket(EventEnum.PlayerPosition, w =>
            {
                w.Write(playerId);
                w.Write(position.X);
                w.Write(position.Y);
                w.Write(position.Z);
                w.Write(rotation.IsIdentity);
                w.Write(rotation.X);
                w.Write(rotation.Y);
                w.Write(rotation.Z);
                w.Write(rotation.W);
            });
        }

        public void WriteDoorOpened(int playerId, Vector3 position)
        {
            AddPacket(EventEnum.DoorOpened, w =>
            {
                w.Write(playerId);
                w.Write(position.X);
                w.Write(position.Y);
                w.Write(position.Z);
            });
        }

        public void WriteDoorClosed(int playerId, Vector3 position)
        {
            AddPacket(EventEnum.DoorClosed, w =>
            {
                w.Write(playerId);
                w.Write(position.X);
                w.Write(position.Y);
                w.Write(position.Z);
            });
        }

        public void WritePlayerJoined(int playerId, string nickname, byte roleType)
        {
            AddPacket(EventEnum.PlayerJoined, w =>
            {
                w.Write(playerId);
                w.Write(nickname);
                w.Write(roleType);
            });
        }

        public void WritePlayerLeft(int playerId)
        {
            AddPacket(EventEnum.PlayerLeft, w =>
            {
                w.Write(playerId);
            });
        }

        public void WriteRoom(string roomName, Vector3 position, Quaternion rotation)
        {
            AddPacket(EventEnum.Room, w =>
            {
                w.Write(roomName);
                w.Write(position.X);
                w.Write(position.Y);
                w.Write(position.Z);

                w.Write(rotation.IsIdentity);
                w.Write(rotation.X);
                w.Write(rotation.Y);
                w.Write(rotation.Z);
                w.Write(rotation.W);
            });
        }

        public void WritePlayerDied(int attackerId, int victimId, string customData)
        {
            DamageTypeDictionary.Values.TryGetValue(customData, out DamageType damageType);
            AddPacket(EventEnum.PlayerDied, w =>
            {
                w.Write(victimId);
                w.Write(attackerId);
                w.Write((byte)damageType);
            });
        }

        public void WriteNtfWave()
        {
            AddPacket(EventEnum.NtfWave, w => { });
        }

        public void WriteNtfMiniWave()
        {
            AddPacket(EventEnum.NtfMiniWave, w => { });
        }

        public void WriteCIWave()
        {
            AddPacket(EventEnum.CIWave, w => { });
        }

        public void WriteCIMiniWave()
        {
            AddPacket(EventEnum.CIMiniWave, w => { });
        }

        public void WriteBallThrown(int playerId)
        {
            AddPacket(EventEnum.BallThrown, w =>
            {
                w.Write(playerId);
            });
        }

        public void WriteHitByBall(int attackerId, int victimId)
        {
            AddPacket(EventEnum.HitByBall, w =>
            {
                w.Write(victimId);
                w.Write(attackerId);
            });
        }

        public void WriteGrenadeThrown(int playerId)
        {
            AddPacket(EventEnum.GrenadeThrown, w =>
            {
                w.Write(playerId);
            });
        }

        public void WriteGrenadeExploded(int playerId, Vector3 position)
        {
            AddPacket(EventEnum.GrenadeExploded, w =>
            {
                w.Write(playerId);
                w.Write(position.X);
                w.Write(position.Y);
                w.Write(position.Z);
            });
        }

        public void WritePickingUpItem(int playerId, string itemId, Vector3 position)
        {
            AddPacket(EventEnum.PickingUpItem, w =>
            {
                w.Write(playerId);
                w.Write(itemId);
                w.Write(position.X);
                w.Write(position.Y);
                w.Write(position.Z);
            });
        }

        public void WriteFlashGrenadeThrown(int playerId)
        {
            AddPacket(EventEnum.FlashGrenadeThrown, w =>
            {
                w.Write(playerId);
            });
        }

        public void WriteFlashGrenadeExploded(int playerId, Vector3 position)
        {
            AddPacket(EventEnum.FlashGrenadeExploded, w =>
            {
                w.Write(playerId);
                w.Write(position.X);
                w.Write(position.Y);
                w.Write(position.Z);
            });
        }

        public void WriteChargingMicroHid(int playerId)
        {
            AddPacket(EventEnum.ChargingMicroHid, w =>
            {
                w.Write(playerId);
            });
        }

        public void WriteCancelChargingMicroHid(int playerId)
        {
            AddPacket(EventEnum.CancelChargingMicroHid, w =>
            {
                w.Write(playerId);
            });
        }

        public void WriteFiringMicroHid(int playerId)
        {
            AddPacket(EventEnum.FiringMicroHid, w =>
            {
                w.Write(playerId);
            });
        }

        public void WritePlayerEscaped(int playerId)
        {
            AddPacket(EventEnum.PlayerEscaped, w =>
            {
                w.Write(playerId);
            });
        }

        public void WritePlayerEscorted(int cufferId, int playerId)
        {
            AddPacket(EventEnum.PlayerEscorted, w =>
            {
                w.Write(playerId);
                w.Write(cufferId);
            });
        }

        public void WriteSCP268Used(int playerId)
        {
            AddPacket(EventEnum.SCP268Used, w =>
            {
                w.Write(playerId);
            });
        }

        public void WriteSCP268Expired(int playerId)
        {
            AddPacket(EventEnum.SCP268Expired, w =>
            {
                w.Write(playerId);
            });
        }

        public void WritePlayerChangedRoles(int playerId, byte role)
        {
            AddPacket(EventEnum.PlayerChangedRoles, w =>
            {
                w.Write(playerId);
                w.Write(role);
            });
        }
    }
}