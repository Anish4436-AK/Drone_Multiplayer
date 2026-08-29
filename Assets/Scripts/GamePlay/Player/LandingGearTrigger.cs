using System;
using Unity.Netcode;
using UnityEngine;

public class LandingGearTrigger : NetworkBehaviour

{
    public DroneMotor playerMovement;

    private int contactCount = 0;

    //private void OnTriggerEnter(Collider other)
    private void OnCollisionEnter(Collision collision)
    {
        // Ignore triggers
        //if (other.collider) return;
        //Debug.Log($"Collision - IsServer: {IsServer}, IsOwner: {IsOwner}");
        contactCount++;
        playerMovement.canArm = true;
        //Debug.Log("canArm");
    }

    private void OnCollisionExit(Collision collision)
    {
        // Ignore triggers
        //if (other.collider) return;

        contactCount--;
        if(contactCount <= 0 )
        {
            contactCount = 0;
            playerMovement.canArm = false;
        }
      
        //Debug.Log("cannotArm");
    }
}
