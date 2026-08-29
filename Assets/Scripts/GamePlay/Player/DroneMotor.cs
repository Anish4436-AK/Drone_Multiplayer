using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class DroneMotor: NetworkBehaviour
{
    private Rigidbody rb;
    [SerializeField] GameObject PlayerDrone;
    //[SerializeField] float PlayerSpeed = 3f;

    [Header("Wings")]
    public GameObject Wing;
    public GameObject Wing2;
    public GameObject Wing3;
    public GameObject Wing4;
    [SerializeField] float Propeller_Speed = 1000f;

    [Header("Forces")]
    [SerializeField] private float thrustForce = 20f;
    [SerializeField] private float yawForce = 5f;
    [SerializeField] private float pitchForce = 8f;
    [SerializeField] private float rollForce = 20f;

    [Header("Stabilization")]
    [SerializeField] private float rotationStabilizeSpeed = 5f;
    //[SerializeField] private float velocityStabilizeSpeed = 2f;
    //[SerializeField] private float hoverForceMultiplier = 1f;

    [Header("Clamp")]
    [SerializeField] private float maxRoll = 0f;
    [SerializeField] private float maxPitch = 0f;

   
    [Header("bool")]
    //public bool isArmed = false;
    public NetworkVariable<bool> isArmed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public bool canArm = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {

        propellers();

        //if (!IsOwner) return;

        if (canArm == true)
        {
            DroneArm();
        }
        //Debug.Log($"canArm = {canArm}");
    }
  
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isArmed.Value)
        {
            ClampRotation();    
            Dynamics();
        }

        //ClampRotation();
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetDroneRotation();
        }
    }
    
    void DroneArm()
    {
        if (canArm && Input.GetKeyDown(KeyCode.Q))
        {
            isArmed.Value = true;
            //Debug.Log("Drone Armed");
        }

        if (canArm&&Input.GetKeyDown(KeyCode.E))
        {
            isArmed.Value = false;
            //Debug.Log("Drone Disarmed");
        }
    }

    void propellers()
    {

        if (isArmed.Value == true)
        {
            Wing.transform.Rotate(0, Propeller_Speed * Time.deltaTime, 0);
            Wing2.transform.Rotate(0, Propeller_Speed * Time.deltaTime, 0);
            Wing3.transform.Rotate(0, Propeller_Speed * Time.deltaTime, 0);
            Wing4.transform.Rotate(0, Propeller_Speed * Time.deltaTime, 0);
        }
    }

    /*  void Dynamics()
      {
          //Thrust
          if (Input.GetKey(KeyCode.W))
          {
              rb.AddForce(transform.up * thrustForce);
              //Debug.Log("up");
          }

          if (Input.GetKey(KeyCode.S))
          {
              rb.AddForce(-transform.up * thrustForce);
              //Debug.Log("Down");
          }

          //Yaw
          if (Input.GetKey(KeyCode.A))
          {
              rb.AddTorque(-Vector3.up * yawForce);
              //Debug.Log("Yaw Left");
          }

          if (Input.GetKey(KeyCode.D))
          {
              rb.AddTorque(Vector3.up * yawForce);
              //Debug.Log("Yaw Right");
          }

          //Pitch
          if (Input.GetKey(KeyCode.I))
          {
              rb.AddTorque(transform.right * pitchForce);
              //Debug.Log("Forward");
          }

          if (Input.GetKey(KeyCode.K))
          {
              rb.AddTorque(-transform.right * pitchForce);
              //Debug.Log("Backwards");
          }

          //Roll
          if (Input.GetKey(KeyCode.J))
          {
              rb.AddTorque(transform.forward * rollForce);
              //Debug.Log("Left");
          }

          if (Input.GetKey(KeyCode.L))
          {
              rb.AddTorque(-transform.forward * rollForce);
              //Debug.Log("Right");           
          }
      }

      void ClampRotation()
      {
          Vector3 rotation = transform.localEulerAngles;

          // Convert angles from 0-360 to -180 to 180
          float pitch = rotation.x;
          float roll = rotation.z;

          if (pitch > 180) pitch -= 360;
          if (roll > 180) roll -= 360;

          // Clamp values
         pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

         roll = Mathf.Clamp(roll, -maxRoll, maxRoll);

          // Apply clamped rotation
          transform.localEulerAngles = new Vector3(pitch, rotation.y, roll);
      }*/

    void Dynamics()
    {
        
        // THRUST / HOVER
         if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(
                transform.up * thrustForce,
                ForceMode.Acceleration
            );
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(
                -transform.up * thrustForce,
                ForceMode.Acceleration
            );
        }
        else
        {
            // Hover against gravity
            rb.AddForce(
                -Physics.gravity,
                ForceMode.Acceleration
            );
        }

        // YAW

        if (Input.GetKey(KeyCode.A))
        {
            rb.AddRelativeTorque(
                Vector3.down * yawForce,
                ForceMode.Acceleration
            );
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rb.AddRelativeTorque(
                Vector3.up * yawForce,
                ForceMode.Acceleration
            );
        }

        // PITCH

        if (Input.GetKey(KeyCode.I))
        {
            rb.AddRelativeTorque(
                Vector3.right * pitchForce,
                ForceMode.Acceleration
            );
        }
        else if (Input.GetKey(KeyCode.K))
        {
            rb.AddRelativeTorque(
                Vector3.left * pitchForce,
                ForceMode.Acceleration
            );
        }
        else
        {
            StabilizePitch();
        }

        // ROLL

        if (Input.GetKey(KeyCode.J))
        {
            rb.AddRelativeTorque(
                Vector3.forward * rollForce,
                ForceMode.Acceleration
            );
        }
        else if (Input.GetKey(KeyCode.L))
        {
            rb.AddRelativeTorque(
                Vector3.back * rollForce,
                ForceMode.Acceleration
            );
        }
        else
        {
            StabilizeRoll();
        }

        // MOVEMENT FROM TILT

        Vector3 forwardMovement =
            transform.forward * transform.forward.y * -thrustForce;

        Vector3 sideMovement =
            transform.right * transform.right.y * -thrustForce;

        rb.AddForce(
            forwardMovement + sideMovement,
            ForceMode.Acceleration
        );
    }

    void StabilizePitch()
    {
        Vector3 rotation = transform.localEulerAngles;

        float pitch = rotation.x;

        if (pitch > 180f)
            pitch -= 360f;

        float correction =
            -pitch * rotationStabilizeSpeed;

        rb.AddRelativeTorque(
            Vector3.right * correction,
            ForceMode.Acceleration
        );
    }

    void StabilizeRoll()
    {
        Vector3 rotation = transform.localEulerAngles;

        float roll = rotation.z;

        if (roll > 180f)
            roll -= 360f;

        float correction =
            -roll * rotationStabilizeSpeed;

        rb.AddRelativeTorque(
            Vector3.forward * correction,
            ForceMode.Acceleration
        );
    }

    void ClampRotation()
    {
        Vector3 rotation = rb.rotation.eulerAngles;

        float pitch = rotation.x;
        float yaw = rotation.y;
        float roll = rotation.z;

        if (pitch > 180f)
            pitch -= 360f;

        if (roll > 180f)
            roll -= 360f;

        pitch = Mathf.Clamp(pitch,-maxPitch,maxPitch);

        roll = Mathf.Clamp(roll,-maxRoll,maxRoll);

        rb.MoveRotation(Quaternion.Euler(pitch,yaw,roll));
    }

    /*void StabilizeDrone()
    {
        Vector3 rotation = rb.rotation.eulerAngles;

        float pitch = rotation.x;
        float roll = rotation.z;

        // Convert 0-360 to -180 to 180
        if (pitch > 180f)
            pitch -= 360f;

        if (roll > 180f)
            roll -= 360f;


        // Only stabilize pitch when I and K are released
        if (!Input.GetKey(KeyCode.I) &&
            !Input.GetKey(KeyCode.K))
        {
            rb.AddRelativeTorque(
                Vector3.right *
                (-pitch * rotationStabilizeSpeed),
                ForceMode.Acceleration
            );
        }


        // Only stabilize roll when J and L are released
        if (!Input.GetKey(KeyCode.J) &&
            !Input.GetKey(KeyCode.L))
        {
            rb.AddTorque(
                Vector3.forward *
                (-roll * rotationStabilizeSpeed),
                ForceMode.Acceleration
            );
        }


        // Reduce unwanted angular movement
        if (!Input.GetKey(KeyCode.I) &&
            !Input.GetKey(KeyCode.K) &&
            !Input.GetKey(KeyCode.J) &&
            !Input.GetKey(KeyCode.L))
        {
            rb.angularVelocity = Vector3.Lerp(
                rb.angularVelocity,
                Vector3.zero,
                rotationStabilizeSpeed * Time.fixedDeltaTime
            );
        }
    }*/

    void ResetDroneRotation()
    {
        PlayerDrone.transform.rotation = Quaternion.Euler(0, 0, 0);  
    }
}
