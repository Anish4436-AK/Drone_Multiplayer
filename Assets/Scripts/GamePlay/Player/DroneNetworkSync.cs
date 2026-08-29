//using Unity.Netcode;
//using UnityEngine;

//public class DroneNetworkSync : NetworkBehaviour
//{
//    private DroneMotor motor;
//    private DroneInputHandler inputHandler;

//    private DroneInput input;

//    private void Awake()
//    {
//        motor = GetComponent<DroneMotor>();
//        inputHandler = GetComponent<DroneInputHandler>();
//    }

//    private void Update()
//    {
//        if (!IsOwner)
//            return;

//        SendInputServerRpc(inputHandler.CurrentInput);
//    }

//    [ServerRpc]
//    //private void SendInputServerRpc(DroneInput input)
//    //{
//    //    motor.SetInput(input);a
//    //}
//}