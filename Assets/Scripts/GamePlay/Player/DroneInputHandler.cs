using Unity.Netcode;
using UnityEngine;

public class DroneInputHandler : NetworkBehaviour
{
    public DroneInput CurrentInput; //{ get; private set; }

    private void Update()
    {
        if (!IsOwner)
            return;

        //DroneInput input = CurrentInput;

        CurrentInput.ThrustUp = Input.GetKey(KeyCode.W);
        CurrentInput.ThrustDown = Input.GetKey(KeyCode.S);

        CurrentInput.YawLeft = Input.GetKey(KeyCode.A);
        CurrentInput.YawRight = Input.GetKey(KeyCode.D);

        CurrentInput.PitchForward = Input.GetKey(KeyCode.I);
        CurrentInput.PitchBackward = Input.GetKey(KeyCode.K);

        CurrentInput.RollLeft = Input.GetKey(KeyCode.J);
        CurrentInput.RollRight = Input.GetKey(KeyCode.L);

        CurrentInput.Arm = Input.GetKey(KeyCode.Q);
        CurrentInput.Disarm = Input.GetKey(KeyCode.E);

        //CurrentInput = input;
    }
}