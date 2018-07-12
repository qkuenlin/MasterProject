using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class FollowPath : MonoBehaviour {
    /// <summary>The path to follow</summary>
    [Tooltip("The path to follow")]
    public CinemachinePathBase m_Path;

    /// <summary>This enum defines the options available for the update method.</summary>
    public enum UpdateMethod
    {
        /// <summary>Updated in normal MonoBehaviour Update.</summary>
        Update,
        /// <summary>Updated in sync with the Physics module, in FixedUpdate</summary>
        FixedUpdate
    };

    /// <summary>When to move the cart, if Velocity is non-zero</summary>
    [Tooltip("When to move the cart, if Velocity is non-zero")]
    public UpdateMethod m_UpdateMethod = UpdateMethod.Update;

    /// <summary>How to interpret the Path Position</summary>
    [Tooltip("How to interpret the Path Position.  If set to Path Units, values are as follows: 0 represents the first waypoint on the path, 1 is the second, and so on.  Values in-between are points on the path in between the waypoints.  If set to Distance, then Path Position represents distance along the path.")]
    public CinemachinePathBase.PositionUnits m_PositionUnits = CinemachinePathBase.PositionUnits.Distance;

    /// <summary>Move the cart with this speed</summary>
    [Tooltip("Start to move the cart with this speed along the path.  The value is interpreted according to the Position Units setting.")]
    public float m_Speed;

    public float m_MinSpeed;

    public float m_MaxSpeed;

    [Tooltip("Gravity Acceleration.  The value is interpreted according to the Position Units setting.")]
    public float m_G = -9.81f;

    [Tooltip("Air Denstiy: Correspond to (drag_coef * air_density * cross-section). The value is interpreted according to the Position Units setting.")]
    public float m_AirDensity = 1.1455f;

    [Tooltip("Air Resistance: Correspond to (drag_coef * air_density * cross-section). The value is interpreted according to the Position Units setting.")]
    public float m_DragCoefficient = 0.2f;

    /// <summary>The cart's current position on the path, in distance units</summary>
    [Tooltip("The position along the path at which the cart will be placed.  This can be animated directly or, if the velocity is non-zero, will be updated automatically.  The value is interpreted according to the Position Units setting.")]
    public float m_Position;

    private float manualAcceleration;

    void FixedUpdate()
    {
        manualAcceleration = 20.0f * Input.GetAxis("Ride Acceleration");

        if (m_UpdateMethod == UpdateMethod.FixedUpdate)
        {
            UpdateSpeed();
            SetCartPosition(m_Position += m_Speed * Time.deltaTime);
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
            SetCartPosition(m_Position);
        else if (m_UpdateMethod == UpdateMethod.Update)
        {
            UpdateSpeed();
            SetCartPosition(m_Position += m_Speed * Time.deltaTime);
        }
    }

    void UpdateSpeed()
    {
        if (m_Path != null)
        {
            float m_UnitPosition = m_Path.StandardizeUnit(m_Position, m_PositionUnits);
            Vector3 t = m_Path.EvaluateTangentAtUnit(m_Position, m_PositionUnits);

            m_Speed += (m_G * t.normalized.y - m_DragCoefficient * m_AirDensity * Mathf.Sqrt(m_Speed) + manualAcceleration) * Time.deltaTime;
            m_Speed = Mathf.Clamp(m_Speed, m_MinSpeed, m_MaxSpeed);
 
            Debug.Log(m_G * t.normalized.y + "   " + (m_DragCoefficient * m_AirDensity) + "   " + manualAcceleration);
        }
    }

    void SetCartPosition(float distanceAlongPath)
    {
        if (m_Path != null)
        {
            m_Position = m_Path.StandardizeUnit(distanceAlongPath, m_PositionUnits);
            transform.position = m_Path.EvaluatePositionAtUnit(m_Position, m_PositionUnits);
            transform.rotation = m_Path.EvaluateOrientationAtUnit(m_Position, m_PositionUnits);
        }
    }
}
