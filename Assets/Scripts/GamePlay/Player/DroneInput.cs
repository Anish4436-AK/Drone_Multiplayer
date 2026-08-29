using Unity.Netcode;

public struct DroneInput : INetworkSerializable
{
    public bool ThrustUp;
    public bool ThrustDown;

    public bool YawLeft;
    public bool YawRight;

    public bool PitchForward;
    public bool PitchBackward;

    public bool RollLeft;
    public bool RollRight;

    public bool Arm;
    public bool Disarm;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref ThrustUp);
        serializer.SerializeValue(ref ThrustDown);

        serializer.SerializeValue(ref YawLeft);
        serializer.SerializeValue(ref YawRight);

        serializer.SerializeValue(ref PitchForward);
        serializer.SerializeValue(ref PitchBackward);

        serializer.SerializeValue(ref RollLeft);
        serializer.SerializeValue(ref RollRight);

        serializer.SerializeValue(ref Arm);
        serializer.SerializeValue(ref Disarm);
    }
}