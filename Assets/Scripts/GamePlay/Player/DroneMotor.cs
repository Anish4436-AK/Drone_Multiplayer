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

    [Header("Rotation Stabilization")]
    [SerializeField] private float stabilizationStrength = 8f;
    [SerializeField] private float stabilizationDamping = 3f;
    [SerializeField] private float maxStabilizationTorque = 10f;

    [Header("Clamp")]
    [SerializeField] private float maxRoll = 0f;
    [SerializeField] private float maxPitch = 0f;

    [Header("Pitch & Roll Limits")]
    [SerializeField] private float maxPitchAngle = 25f;
    [SerializeField] private float maxRollAngle = 25f;
    [SerializeField] private float limitStrength = 12f;
    [SerializeField] private float limitDamping = 4f;

    [Header("Drone Motor Sound")]
    [SerializeField] private AudioSource motorAudio;


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

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (motorAudio != null)
            {
                motorAudio.Play();
                Debug.Log("Testing drone sound: " + motorAudio.clip.name);
            }
            else
            {
                Debug.LogError("Motor AudioSource is not assigned!");
            }
        }
    }
  
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isArmed.Value)
        {
         //   ClampRotation();    
            Dynamics();
            ClampPitchAndRoll();
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
            UpdateMotorSound(true);
            //Debug.Log("Drone Armed");
        }

        if (canArm&&Input.GetKeyDown(KeyCode.E))
        {
            isArmed.Value = false;
            UpdateMotorSound(false);
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

    private void UpdateMotorSound(bool armed)
    {
        if (motorAudio == null)
            return;

        if (armed)
        {
            if (!motorAudio.isPlaying)
                motorAudio.Play();

        }
        else
        {
            if (motorAudio.isPlaying)
                motorAudio.Stop();
        }
    }

    public override void OnNetworkSpawn()
    {
        isArmed.OnValueChanged += OnArmedChanged;

        // Handle the initial state for late-joining clients.
        UpdateMotorSound(isArmed.Value);
    }

    public override void OnNetworkDespawn()
    {
        isArmed.OnValueChanged -= OnArmedChanged;

        if (motorAudio != null)
            motorAudio.Stop();
    }

    private void OnArmedChanged(bool previousValue, bool newValue)
    {
        UpdateMotorSound(newValue);
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
        // =====================================================
        // ALTITUDE
        // =====================================================

        float verticalInput = 0f;

        if (Input.GetKey(KeyCode.W))
            verticalInput = 1f;

        if (Input.GetKey(KeyCode.S))
            verticalInput = -1f;

        // Always counter gravity.
        // This prevents the drone from falling when tilted.
        rb.AddForce(
            Vector3.up * -Physics.gravity.y,
            ForceMode.Acceleration
        );

        // Additional vertical movement
        if (verticalInput != 0f)
        {
            rb.AddForce(
                Vector3.up * verticalInput * thrustForce,
                ForceMode.Acceleration
            );
        }


        // =====================================================
        // YAW
        // =====================================================

        if (Input.GetKey(KeyCode.A))
        {
            rb.AddRelativeTorque(
                Vector3.down * yawForce,
                ForceMode.Acceleration
            );
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb.AddRelativeTorque(
                Vector3.up * yawForce,
                ForceMode.Acceleration
            );
        }


        // =====================================================
        // PITCH
        // =====================================================

        if (Input.GetKey(KeyCode.I))
        {
            rb.AddRelativeTorque(
                Vector3.left * pitchForce,
                ForceMode.Acceleration
            );
        }

        if (Input.GetKey(KeyCode.K))
        {
            rb.AddRelativeTorque(
                Vector3.right * pitchForce,
                ForceMode.Acceleration
            );
        }


        // =====================================================
        // ROLL
        // =====================================================

        if (Input.GetKey(KeyCode.J))
        {
            rb.AddRelativeTorque(
                Vector3.back * rollForce,
                ForceMode.Acceleration
            );
        }

        if (Input.GetKey(KeyCode.L))
        {
            rb.AddRelativeTorque(
                Vector3.forward * rollForce,
                ForceMode.Acceleration
            );
        }


        // =====================================================
        // HORIZONTAL MOVEMENT FROM TILT
        // =====================================================

        Vector3 horizontalTilt =
            new Vector3(
                transform.up.x,
                0f,
                transform.up.z
            );

        Vector3 horizontalMovement =
            -horizontalTilt * thrustForce;

        rb.AddForce(
            horizontalMovement,
            ForceMode.Acceleration
        );


        // =====================================================
        // ROTATION STABILIZATION
        // =====================================================

        StabilizeDrone();
    }

    void StabilizeDrone()
    {
        // Direction needed to bring drone's UP
        // back toward world UP.
        Vector3 tiltError =
            Vector3.Cross(
                transform.up,
                Vector3.up
            );

        // Angular velocity in local drone space
        Vector3 localAngularVelocity =
            transform.InverseTransformDirection(
                rb.angularVelocity
            );

        // Do not stabilize yaw.
        localAngularVelocity.y = 0f;

        Vector3 worldAngularVelocity =
            transform.TransformDirection(
                localAngularVelocity
            );

        // PD stabilization
        Vector3 stabilizationTorque =
            (tiltError * stabilizationStrength)
            - (worldAngularVelocity * stabilizationDamping);

        stabilizationTorque =
            Vector3.ClampMagnitude(
                stabilizationTorque,
                maxStabilizationTorque
            );

        rb.AddTorque(
            stabilizationTorque,
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

    private void ClampPitchAndRoll()
    {
        // Get current rotation relative to the drone's starting orientation.
        Vector3 localAngles = transform.localEulerAngles;

        float pitch = localAngles.x;
        float roll = localAngles.z;

        // Convert Unity's 0–360 angles into -180 to 180.
        if (pitch > 180f) pitch -= 360f;
        if (roll > 180f) roll -= 360f;

        Vector3 localAngularVelocity =
            transform.InverseTransformDirection(rb.angularVelocity);

        float pitchTorque = 0f;
        float rollTorque = 0f;

        // Pitch limit
        if (Mathf.Abs(pitch) > maxPitchAngle)
        {
            float excess = Mathf.Abs(pitch) - maxPitchAngle;

            pitchTorque = -Mathf.Sign(pitch) *
                          (excess * limitStrength);

            pitchTorque -= localAngularVelocity.x * limitDamping;
        }

        // Roll limit
        if (Mathf.Abs(roll) > maxRollAngle)
        {
            float excess = Mathf.Abs(roll) - maxRollAngle;

            rollTorque = -Mathf.Sign(roll) *
                         (excess * limitStrength);

            rollTorque -= localAngularVelocity.z * limitDamping;
        }

        rb.AddRelativeTorque(
            new Vector3(pitchTorque, 0f, rollTorque),
            ForceMode.Acceleration
        );
    }
    void ResetDroneRotation()
    {
        PlayerDrone.transform.rotation = Quaternion.Euler(0, 0, 0);  
    }
}
