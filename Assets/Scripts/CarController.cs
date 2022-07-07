
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using Unity.Netcode;

public class CarController : NetworkBehaviour
{
    [Serializable]
    public struct Wheel
    {
        public WheelCollider wheelCollider;
        public GameObject wheelMesh;
    }

    [SerializeField] private float maxTorque;
    [SerializeField] private float currentMotorTorque;
    [SerializeField] private float currentDriveTorque;
    [SerializeField] private float brakeTorque = 1000.0f;
    [SerializeField] private float desiredTurnRadius = 6.0f;
    [SerializeField] private float smoothing = 0.01f;
    [SerializeField] private Wheel[] wheels;

    [Header("Engine")]
    [SerializeField] private AnimationCurve torqueCurve;
    [SerializeField] private float experimentalTorqueConstant;
    [SerializeField] private float currentRPM;
    [SerializeField] private float minRPM = 1000.0f;
    [SerializeField] private float maxRPM = 9000.0f;

    [Header("Transmission")]
    [SerializeField] private float[] gears;
    [SerializeField] private List<float> maxVelocities = new List<float>();
    [SerializeField] private float transmissionEfficiency = 1.0f;
    [SerializeField] private float differentialRatio = 3.4f;
    [SerializeField] private float currentVelocity = 0.0f;
    [SerializeField] private int currentGear;
    [SerializeField] private float currentGearRatio;
    [SerializeField] private TMP_Text velocityField;
    [SerializeField] private TMP_Text rpmField;
    [SerializeField] private TMP_Text currentGearField;
    [SerializeField] private TMP_Text wheelRPMField;

    [Header("Aerodynamics")]
    [SerializeField] private float downforce = 10.0f;

    [Header("Sounds")]
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private AudioSource engineRedlineSound;
    [SerializeField] private AudioSource engineDecelSound;
    [SerializeField] private AudioSource engineTurboSound;
    [SerializeField] private float pitchMin;
    [SerializeField] private float pitchMax;
    [SerializeField] private float rpmSmoothingTime = 0.1f;

    private float slope = 0.0f;

    [SerializeField] private Transform centerOfMass;
     
    [SerializeField] private InputManager input;
    private Rigidbody rb;
    private Quaternion curWheelRot;
    private Vector3 curWheelPos;
    private float curVel = 0.0f;
    private float curVelRPM = 0.0f;


    private void Start()
    {
        TryGetComponent(out rb);

        rb.centerOfMass = centerOfMass.localPosition;

        foreach (Wheel wheel in wheels)
        {
            wheel.wheelCollider.ConfigureVehicleSubsteps(10.0f, 8, 6);
        }
        
        currentGear = 0;

        CalculateMaxVelocities();
        slope = (pitchMax - pitchMin) / (maxRPM - minRPM);
    }

    private void OnEnable()
    {
        InputManager.ShiftUp += ShiftGearUp;
        InputManager.ShiftDown += ShiftGearDown;
    }

    private void OnDisable()
    {
        InputManager.ShiftUp -= ShiftGearUp;
        InputManager.ShiftDown -= ShiftGearDown;
    }

    private void Update()
    {
        Steering();
        Braking();
        CalculateRPM();
        CalculateTorque();

        currentVelocity = rb.velocity.magnitude * 3.6f;
        velocityField.text = "KM/H: " + Convert.ToInt16(currentVelocity);
        rpmField.text = "RPM: " + Convert.ToInt32(currentRPM);
        currentGearField.text = "Gear: " + currentGear;

        SetEngineSoundPitch();
    }

    private void FixedUpdate()
    {
        wheels[2].wheelCollider.motorTorque = currentDriveTorque / 2;
        wheels[3].wheelCollider.motorTorque = currentDriveTorque / 2;

        rb.AddForce(Vector3.down * downforce, ForceMode.Force);

        UpdateWheels();
    }

    private void Steering()
    {
        if (input.steeringInput > 0)
        {
            wheels[0].wheelCollider.steerAngle = Mathf.SmoothDamp(wheels[0].wheelCollider.steerAngle, Mathf.Rad2Deg * Mathf.Atan(2.55f / (desiredTurnRadius + (1.5f / 2))) * input.steeringInput, ref curVel, smoothing);
            wheels[1].wheelCollider.steerAngle = Mathf.SmoothDamp(wheels[1].wheelCollider.steerAngle, Mathf.Rad2Deg * Mathf.Atan(2.55f / (desiredTurnRadius - (1.5f / 2))) * input.steeringInput, ref curVel, smoothing);
        }
        else if (input.steeringInput < 0)
        {
            wheels[0].wheelCollider.steerAngle = Mathf.SmoothDamp(wheels[0].wheelCollider.steerAngle, Mathf.Rad2Deg * Mathf.Atan(2.55f / (desiredTurnRadius - (1.5f / 2))) * input.steeringInput, ref curVel, smoothing);
            wheels[1].wheelCollider.steerAngle = Mathf.SmoothDamp(wheels[1].wheelCollider.steerAngle, Mathf.Rad2Deg * Mathf.Atan(2.55f / (desiredTurnRadius + (1.5f / 2))) * input.steeringInput, ref curVel, smoothing);
        }
        else
        {
            wheels[0].wheelCollider.steerAngle = Mathf.SmoothDamp(wheels[0].wheelCollider.steerAngle, 0.0f, ref curVel, smoothing);
            wheels[1].wheelCollider.steerAngle = Mathf.SmoothDamp(wheels[1].wheelCollider.steerAngle, 0.0f, ref curVel, smoothing);
        }
    }

    private void Braking()
    {
        if (input.controls.Main.CarBreaking.ReadValue<float>() > 0)
        {
            wheels[0].wheelCollider.brakeTorque = brakeTorque;
            wheels[1].wheelCollider.brakeTorque = brakeTorque;
            //wheels[2].wheelCollider.brakeTorque = brakeTorque;
            //wheels[3].wheelCollider.brakeTorque = brakeTorque;
        }
        else
        {
            wheels[0].wheelCollider.brakeTorque = 0.0f;
            wheels[1].wheelCollider.brakeTorque = 0.0f;
        }
    }

    private void CalculateRPM()
    {
        currentRPM = Mathf.SmoothDamp(currentRPM, Mathf.Abs(((wheels[2].wheelCollider.rpm + wheels[3].wheelCollider.rpm) / 2) * differentialRatio * currentGearRatio), ref curVelRPM, rpmSmoothingTime);

        wheelRPMField.text = "Rear Wheel RPM: " + (wheels[2].wheelCollider.rpm + wheels[3].wheelCollider.rpm) / 2;

        if (currentRPM < minRPM)
        {
            currentRPM = minRPM;
        }
        if (currentRPM >= maxRPM)
        {
            currentRPM = maxRPM;
            engineRedlineSound.mute = false;
        }
        else
        {
            engineRedlineSound.mute = true;
        }
        if(currentRPM != minRPM && input.accelerationInput == 0)
        {
            engineSound.mute = true;
            engineDecelSound.mute = false;
        }
        else
        {
            engineDecelSound.mute = true;
            engineSound.mute = false;
        }
    }

    private void CalculateTorque()
    {
        maxTorque = experimentalTorqueConstant;//torqueCurve.Evaluate(currentRPM);
   
        currentMotorTorque = maxTorque * input.accelerationInput;

        currentDriveTorque = currentMotorTorque * currentGearRatio * differentialRatio * transmissionEfficiency;

        if (currentVelocity >= maxVelocities[currentGear])
        {
            currentDriveTorque = 0.0f;
        }
    }

    private void UpdateWheels()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.wheelCollider.GetWorldPose(out curWheelPos, out curWheelRot);

            wheel.wheelMesh.transform.rotation = curWheelRot;
            wheel.wheelMesh.transform.position = curWheelPos;
        }
    }

    private void ShiftGearUp()
    {
        if (currentGear >= gears.Length - 1) { return; }
        currentGear++;
        currentGearRatio = gears[currentGear];
        engineTurboSound.Play();
    }

    private void ShiftGearDown()
    {
        if (currentGear == gears[0]) { return; }
        currentGear--;
        currentGearRatio = gears[currentGear];
        engineTurboSound.Play();
    }

    private void CalculateMaxVelocities()
    {
        foreach (float currentRatio in gears)
        {
            maxVelocities.Add(Convert.ToInt16((maxRPM * (1.0f / currentRatio) * (1.0f / differentialRatio)) * wheels[2].wheelCollider.radius * ((2.0f * Mathf.PI) / 60.0f) * 3.6f));
        }
    }

    private void SetEngineSoundPitch()
    {
        float buffer = pitchMin + slope * (currentRPM - minRPM);
        engineSound.pitch = buffer;
        engineDecelSound.pitch = buffer;
    }
}
