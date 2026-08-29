using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PlayerRole
{
    Instructor,
    Trainee
}

public class PlayerData
{
    public ulong ClientId;
    public PlayerRole Role;
    public int DroneSelected = -1;
}
