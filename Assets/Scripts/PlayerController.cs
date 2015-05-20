using UnityEngine;
using System.Collections.Generic;
using System;
using Common;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputController))]
[RequireComponent(typeof(LedgeSensor))]
public class PlayerController : MonoBehaviour
{
    #region Inspector

    [Tooltip("The player's point of view")]
    public GameObject Pov;

    [Tooltip("The character's rotation speed, in degree per second")]
    public float RotationSpeed = 360F;

    #endregion

    private CharacterController _characterController;

    private IInputController _inputController;

    private LedgeSensor _ledgeSensor;

    private PlayerAnimatorController _animatorController;

    private Vector3 _velocity;

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
        _inputController = GetComponent<IInputController>();
        _ledgeSensor = GetComponent<LedgeSensor>();
        _animatorController = GetComponent<PlayerAnimatorController>();
    }
	
	void Update () 
    {
        var input = GetTransformedInput();
        _animatorController.InputMove = input.Move.magnitude;

        ApplyGroundRotation(input.Move);

        var localVelocity = _animatorController.Velocity;
        var velocity = Vector3.zero;
        velocity += transform.forward * localVelocity.z;
        velocity += transform.right * localVelocity.x;
        velocity += transform.up * localVelocity.y;

        _characterController.Move(velocity * Time.deltaTime);
	}

    private TransformedInput GetTransformedInput()
    {
        Vector3 forwardAxis, strafeAxis;
        GetGroundAxis(out forwardAxis, out strafeAxis);

        return new TransformedInput()
        {
            Forward = forwardAxis * _inputController.Forward,
            Strafe = strafeAxis * _inputController.Strafe,
        };
    }

    private void ApplyGroundRotation(Vector3 move)
    {
        if (move.magnitude != 0F)
        {
            var desiredRotation = Quaternion.LookRotation(move);
            var desiredDeltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, desiredRotation.eulerAngles.y);

            var maxDeltaAngle = RotationSpeed * _animatorController.RotationLerpFactor * Time.deltaTime;

            float actualDeltaAngle;
            if (desiredDeltaAngle > maxDeltaAngle)
            {
                actualDeltaAngle = maxDeltaAngle;
            }
            else if (desiredDeltaAngle < -maxDeltaAngle)
            {
                actualDeltaAngle = -maxDeltaAngle;
            }
            else
            {
                actualDeltaAngle = desiredDeltaAngle;
            }

            transform.Rotate(Vector3.up, actualDeltaAngle, Space.Self);
        }
    }

    #region Trucs internes

    private class TransformedInput
    {
        public Vector3 Forward
        {
            get;
            set;
        }

        public Vector3 Strafe
        {
            get;
            set;
        }

        public Vector3 Direction
        {
            get
            {
                return Move.normalized;
            }
        }

        public Vector3 Move
        {
            get
            {
                return Forward + Strafe;
            }
        }
    }
    
    #endregion

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
