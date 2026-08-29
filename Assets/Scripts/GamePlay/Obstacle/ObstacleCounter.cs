using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class ObstacleCounter : MonoBehaviour
{
   public bool Triggered = false;
   
   
    void OnTriggerEnter(Collider other)
    {
        if (Triggered)
        {
            return;
        }

        if (other.gameObject.tag == "Player")
        {
            ObstacleManagerCounter Counter = GetComponentInParent<ObstacleManagerCounter>();

            if (Counter != null)
            {
                Counter.obstacleCount++;
                if (SessionManager.Instance.SelectedDifficulty==SessionManager.Difficulty.Hard)
                {
                    Counter.isDifficult = true;
                    Counter.obstacleSpawn.ObstacleSpawner();
                }
            //Debug.Log("Obstacle Count: " + Counter.obstacleCount);                
            }
            Counter.isDifficult = false;
            Triggered = true;
        }
       
    }
}
