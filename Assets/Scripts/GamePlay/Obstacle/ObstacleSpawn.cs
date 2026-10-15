using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawn : MonoBehaviour
{
    [SerializeField] public int ObstacleCount = 0;
    [SerializeField] Transform ObstacleManager;
    [SerializeField] GameObject obstacle;
    public ObstacleManagerCounter OMC;

    [Header ("ObstacleManagment")]
    [SerializeField] public float[] lanes = { -10f, -20f, 0, 10f, 20f};
    [SerializeField] public float[] height = { 0, 3f, 6f, 10f};
    //[SerializeField] public int numberOfObstaclesAtS1 = 2;
    //[SerializeField] public int numberOfObstaclesAtS2 = 5;
    //[SerializeField] public int numberOfObstaclesAtS3 = 5;

    [SerializeField] public int numberOfObstaclesAt_Difficult = 10;

    [SerializeField] float ObstacleSpawnDistance = 30f;

    [Header("Stages")]
    public bool S2= false;
    public bool S3 = false;

    [Header("Modes")]
    public bool easyMode = false;
    public bool difficultMode = false;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionManager.Instance.SelectedDifficulty == SessionManager.Difficulty.Hard)
        {
            ObstacleSpawner();
        }

        if (SessionManager.Instance.SelectedDifficulty == SessionManager.Difficulty.Easy)
        {
            ObstacleSpawnAtS1();
        }
    }

    private void Update()
    {
        if (ObstacleCount == SessionManager.Instance.S1ObstacleCount && !S2)
        {
            ObstacleSpawnAtS2();
        }

        if (ObstacleCount == SessionManager.Instance.S1ObstacleCount + SessionManager.Instance.S2ObstacleCount && !S3)
        {
            ObstacleSpawnAtS3();
        }
    }

    public void ObstacleSpawner()
    {
        GameObject Obstacle = Instantiate(obstacle, new Vector3(lanes[UnityEngine.Random.Range(0, lanes.Length)], height[UnityEngine.Random.Range(0, height.Length)], transform.position.z + ObstacleSpawnDistance), Quaternion.identity, ObstacleManager);
        transform.position += new Vector3(0, 0, ObstacleSpawnDistance);

        Debug.Log("Spawning Obstacle at: " + transform.position);
    }

    void Easy()
    {
        ObstacleSpawnAtS1();
        ObstacleSpawnAtS2();
        ObstacleSpawnAtS3();

    }

    void ObstacleSpawnAtS1()
    {
        //Vector3 ObstacleDistance= new Vector3(0, 0, ObstacleSpawnDistance);
        for (int i = 0; i < SessionManager.Instance.S1ObstacleCount; i++)
        {
           GameObject Obstacle= Instantiate(obstacle, new Vector3(transform.position.x, transform.position.y, transform.position.z + 20f), Quaternion.identity, ObstacleManager);
            transform.position += new Vector3(0, 0, ObstacleSpawnDistance);
            //Obstacle.transform.SetParent(transform );
        }        
    }

    void ObstacleSpawnAtS2()
    {
        float spawnDistanceS2 = ObstacleSpawnDistance * (SessionManager.Instance.S1ObstacleCount);
        for (int i = 0; i < SessionManager.Instance.S2ObstacleCount; i++)
        {
            GameObject Obstacle = Instantiate(obstacle, new Vector3(lanes[UnityEngine.Random.Range(0, lanes.Length)], transform.position.y, transform.position.z + spawnDistanceS2), Quaternion.identity, ObstacleManager);
            transform.position += new Vector3(0, 0, ObstacleSpawnDistance);
            //Obstacle.transform.SetParent(transform, true);
        }      
        S2 = true;
    }

    void ObstacleSpawnAtS3()
    {
        float spawnDistanceS3 = ObstacleSpawnDistance * (SessionManager.Instance.S1ObstacleCount + SessionManager.Instance.S2ObstacleCount);
        for (int i = 0; i < SessionManager.Instance.S3ObstacleCount; i++)
        {
            GameObject Obstacle = Instantiate(obstacle, new Vector3(lanes[UnityEngine.Random.Range(0, lanes.Length)], height[UnityEngine.Random.Range(0, height.Length)], transform.position.z + spawnDistanceS3), Quaternion.identity, ObstacleManager);
            transform.position += new Vector3(0, 0, ObstacleSpawnDistance);
            //Obstacle.transform.SetParent(transform, true);
        }
        S3 = true;
    }   
}
