using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManagerCounter : MonoBehaviour
{
    public ObstacleSpawn obstacleSpawn;
    public int obstacleCount = 0;

    public bool isDifficult = false;
        
    void Update()
    {
        obstacleSpawn.ObstacleCount = obstacleCount;
    }
}
 