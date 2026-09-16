namespace SLGameLogger
{
    public enum EventEnum
    {
        None,
        MapLCZ, MapHCZ, MapEZ,
        PlayerPosition,
        DoorClosed,
        DoorOpened,
        PlayerDied,
        NtfWave,
        NtfMiniWave,
        CIWave,
        CIMiniWave,
        BallThrown,
        BallBounced,
        HitByBall,
        GrenadeThrown,
        GrenadeExploded,
        PickedUpItem,
        DroppedItem,
        Room,
        PickingUpItem,
        FlashGrenadeThrown,
        FlashGrenadeExploded,
        ChargingMicroHid,
        CancelChargingMicroHid,
        FiringMicroHid,
        PlayerEscaped,
        PlayerEscorted,
        SCP268Used,
        SCP268Expired,
        NewTick, // Packet
        PlayerJoined,
        PlayerLeft,
        PlayerChangedRoles,
    }
}