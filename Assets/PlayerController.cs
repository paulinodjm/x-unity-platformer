using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputController))]
[RequireComponent(typeof(LedgeSensor))]
public class PlayerController : MonoBehaviour
{
    #region Inspector

    [Tooltip("The player's point of view")]
    public GameObject Pov;

    [Tooltip("Walk parameters")]
    public GroundedMovementsInfo WalkParameters = new GroundedMovementsInfo()
    {
        Speed = 5F
    };

    #endregion

    private CharacterController _characterController;

    private InputController _inputController;

    private LedgeSensor _ledgeSensor;

    /// <summary>
    /// Returns the current player point of view.
    /// </summary>
    /// <returns>The point of view</returns>
    public Transform GetPovTransform()
    {
        return (Pov != null) ? Pov.transform : transform;
    }

    /// <summary>
    /// Determines the movement axis when grounded.
    /// </summary>
    /// <param name="move">The move axis</param>
    /// <param name="strafe">The strafe axis</param>
    public void GetGroundAxis(out Vector3 move, out Vector3 strafe)
    {
        var pov = GetPovTransform();

        move = pov.forward;
        move.y = 0F;
        move.Normalize();

        strafe = pov.right;
        strafe.y = 0F;
        strafe.Normalize();
    }

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _inputController = GetComponent<InputController>();
        _ledgeSensor = GetComponent<LedgeSensor>();
    }
	
	void Update () 
    {
        if (HandleFallingLedge())
            return;

        var velocity = _characterController.velocity;
        CalcVelocity(ref velocity, WalkParameters);

        velocity *= Time.deltaTime;
        velocity.y = -5;
        _characterController.Move(velocity);
	}

    private void CalcVelocity(ref Vector3 velocity, GroundedMovementsInfo parameters)
    {
        Vector3 forwardAxis, strafeAxis;
        GetGroundAxis(out forwardAxis, out strafeAxis);

        forwardAxis *= _inputController.Forward;
        strafeAxis *= _inputController.Strafe;

        float speed;
        var desiredDirection = (forwardAxis + strafeAxis).normalized;
        if (desiredDirection.magnitude > 0F)
        {
            var desiredSpeed = (forwardAxis + strafeAxis).magnitude * parameters.Speed;
            var strafeDirection = Vector3.Cross(desiredDirection, Vector3.up);

            var actualMove = Vector3.Project(velocity, desiredDirection);
            var actualStrafe = Vector3.Project(velocity, strafeDirection);

            // -> move (forward) velocity
            speed = actualMove.magnitude;
            if (actualMove.normalized == -desiredDirection)
            {
                speed = -speed;
            }

            if (speed <= desiredSpeed)
            {
                speed += parameters.Acceleration * Time.deltaTime;
                if (speed > desiredSpeed)
                {
                    speed = desiredSpeed;
                }
            }
            else
            {
                speed -= parameters.Friction * Time.deltaTime;
                if (speed < desiredSpeed)
                {
                    speed = desiredSpeed;
                }
            }
            actualMove = desiredDirection * speed;

            // -> strafe velocity
            speed = actualStrafe.magnitude;
            if (speed > 0F)
            {
                speed -= parameters.Friction * Time.deltaTime;
                if (speed < 0F)
                {
                    speed = 0F;
                }
                actualStrafe = actualStrafe.normalized * speed;
            }

            // -> net velocity
            velocity = actualMove + actualStrafe;
        }
        else
        {
            var velocityDirection = velocity.normalized;
            speed = velocity.magnitude;

            speed -= parameters.Friction * Time.deltaTime;
            if (speed < 0F)
            {
                speed = 0F;
            }

            velocity = velocityDirection * speed;
        }
    }

    private bool HandleFallingLedge()
    {
        var fallingLedge = _ledgeSensor.GetFallingLedge();
        if (fallingLedge != null)
        {
            transform.position = fallingLedge.TargetPosition;
            return true;
        }

        return false;
    }

    [Serializable]
    public struct GroundedMovementsInfo
    {
        [Tooltip("The target speed when grounded")]
        public float Speed;

        [Tooltip("The acceleration rate when grounded")]
        public float Acceleration;

        [Tooltip("The friction rate when grounded")]
        public float Friction;
    }
}
