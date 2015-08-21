﻿using UnityEngine;
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

    [Tooltip("Walk parameters")]
    public GroundedMovementsInfo WalkParameters = new GroundedMovementsInfo()
    {
        Speed = 5F,
    };

    [Tooltip("Fall parameters")]
    public GroundedMovementsInfo FallParameters = new GroundedMovementsInfo()
    {
        Speed = 5F,
    };

    [Tooltip("The gravity [0; infinity]")]
    public float Gravity;

    [Tooltip("The jump force [0; infinity]")]
    public float JumpForce;

    [Tooltip("The maximum speed from which the character can instantly stop (ie: to prevent falling from a ledge)")]
    public float InstantStopThresholdSpeed = 1F;

    [Tooltip("The speed the character is pushed to fall of a ledge")]
    public float PushToLedgeSpeed = 5F;

    [Tooltip("The maximum height the character can climb off without falling")]
    public float MaxClimbDownHeight;

    #endregion

    private CharacterController _characterController;

    private IInputController _inputController;

    private GroundedLedgeBehaviour _groundLedgeSensor;

    private PlayerAnimationController _animationController;

    private bool _isFrozen;

    private Vector3 _velocity;

    private bool _isPreviouslyGrounded;

    private string _nextState = "OnGround";

    private bool _isGrounded;

    public string CurrentState
    {
        get;
        private set;
    }

    public void SetState(string name)
    {
        _nextState = name;
    }

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
        _groundLedgeSensor = GetComponent<GroundedLedgeBehaviour>();
        _animationController = GetComponent<PlayerAnimationController>();
    }

    void Update()
    {
        if (_isFrozen)
        {
            _characterController.Move(new Vector3(0, -50F, 0));
            return;
        }

        if (_nextState != null)
        {
            CurrentState = _nextState;
            _nextState = null;
        }

        // update
        BroadcastMessage(CurrentState + "Update", SendMessageOptions.RequireReceiver);
    }

    protected void OnGroundUpdate()
    {
        GroundedLedgeBehaviour.ILowerLedge nearestLedge;
        if (HandleNearestLedge(out nearestLedge))
        {
            return;
        }

        var velocity = _velocity;
        var input = GetTransformedInput();

        var moveParameters = _isGrounded ? WalkParameters : FallParameters;
        CalcVelocity(ref velocity, moveParameters, input);

        // ledges
        if (HandleFallingLedge(nearestLedge, ref velocity, input.Direction))
        {
            return;
        }

        // jump
        if (_isGrounded)
        {
            if (_inputController.Jump)
            {
                velocity.y = JumpForce;
                _isGrounded = false;
            }
            else
            {
                velocity.y = -50;
            }
        }
        else
        {
            velocity.y -= Gravity * Time.deltaTime;
        }

        var previousGrounded = _characterController.isGrounded;
        _characterController.Move(velocity * Time.deltaTime);
        _velocity = _characterController.velocity;
        if (previousGrounded == _isGrounded)
        {
            _isGrounded = _characterController.isGrounded;
        }

        // apply animation
        _animationController.UpdateAnimation(
            _isGrounded ? input.Move : transform.forward,
            _isGrounded
        );
    }

    void Freeze()
    {
        _isFrozen = true;
        _velocity = Vector3.zero;
    }

    void Unfreeze(float gravity)
    {
        _isFrozen = false;
        _velocity.y = -gravity;
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

    private void CalcVelocity(ref Vector3 velocity, GroundedMovementsInfo parameters, TransformedInput input)
    {
        Vector3 horizontalVelocity;
        float speed;
        var desiredDirection = input.Direction;
        if (desiredDirection.magnitude > 0F)
        {
            var desiredSpeed = (input.Forward + input.Strafe).magnitude * parameters.Speed;
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
            horizontalVelocity = actualMove + actualStrafe;
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

            horizontalVelocity = velocityDirection * speed;
        }

        velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
    }

    private bool HandleNearestLedge(out GroundedLedgeBehaviour.ILowerLedge nearestLedge)
    {
        nearestLedge = FindNearestFallingLedge();
        if (nearestLedge == null)
            return false;

        if (nearestLedge.DownPosition != null)
        {
            if (nearestLedge.UpPosition != null)
            {
                // choix de la direction ici
                var fallDirection = nearestLedge.IsGrounded ?
                    nearestLedge.GrabPosition.PerpendicularGrabDirection : -nearestLedge.GrabPosition.PerpendicularGrabDirection;

                var playerDirection = _velocity;
                if (playerDirection.x != 0.0F || playerDirection.z != 0.0F)
                {
                    playerDirection.y = 0.0F;
                    playerDirection.Normalize();
                }
                else
                {
                    playerDirection = transform.forward;
                }

                if (Vector3.Dot(fallDirection, playerDirection) > 0)
                {
                    // fall naturally from the ledge
                    return false;
                }
                else
                {
                    transform.position = nearestLedge.UpPosition.Value;
                    _velocity = new Vector3(0, -50, 0);
                    _animationController.SetLedgeAnimation(
                        -fallDirection,
                        (nearestLedge.GrabPosition.PerpendicularGrabDistance
                        + Vector3.Distance(nearestLedge.GrabPosition.Value, transform.position)) / 2
                    );
                    Freeze();
                    _isGrounded = true;
                }
            }
            else
            {
                // descente forcée
                if (!_isGrounded)
                {
                    nearestLedge = null;
                }
                return false;
            }
        }
        else
        {
            if (nearestLedge.UpPosition != null)
            {
                // montée forcée
                transform.position = nearestLedge.UpPosition.Value;
                _velocity = new Vector3(0, -50, 0);
                _isGrounded = true;
            }
            else
            {
                // nothing to do (should be impossible)
                nearestLedge = null;
                return false;
            }
        }

        return true;
    }

    private bool HandleFallingLedge(GroundedLedgeBehaviour.ILowerLedge nearestLedge, ref Vector3 velocity, Vector3 input)
    {
        if (nearestLedge == null)
            return false;

        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        // si le personnage a les pieds dans le vide, le pousse pour le faire tomber
        if (nearestLedge.UpPosition == null || !nearestLedge.IsGrounded)
        {
            PushToLedge(nearestLedge, ref velocity);
            _isGrounded = false;
            return false;
        }

        if (!_isGrounded) // la suite n'est que pour quand il est au sol
            return false;

        // Arrêt en position de déséquilibre
        if (horizontalVelocity.magnitude <= InstantStopThresholdSpeed && input == Vector3.zero)
        {
            transform.position = nearestLedge.UpPosition.Value;
            _velocity = new Vector3(0, -50, 0);
            _animationController.SetLedgeStopAnimation();
            Freeze();
            _isGrounded = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Look for the nearest falling ledge (the ledge the character must interact with)
    /// </summary>
    /// <returns>The nearest falling ledge</returns>
    private GroundedLedgeBehaviour.ILowerLedge FindNearestFallingLedge()
    {
        var shortestDistance = Single.MaxValue;
        GroundedLedgeBehaviour.ILowerLedge nearestLedge = null;

        foreach (var lowerLedge in _groundLedgeSensor.LowerLedges)
        {
            // skip the ledges too far to interact
            if (lowerLedge.IsGrounded)
            {
                if (lowerLedge.GrabPosition.LedgeDistance > _groundLedgeSensor.FallDistance)
                    continue;
            }
            else
            {
                if (lowerLedge.GrabPosition.LedgeDistance > _characterController.radius)
                    continue;
            }

            // andle ledge angles
            if (!lowerLedge.GrabPosition.IsInFront)
            {
                var skipIt = true;
                foreach (var ledge in _groundLedgeSensor.LowerLedges)
                {
                    if (ledge == lowerLedge)
                        continue;

                    if (!lowerLedge.GrabPosition.Ledge.IsConnectedTo(ledge.GrabPosition.Ledge, 0.02F))
                        continue;

                    if (lowerLedge.IsGrounded != ledge.IsGrounded)
                        continue;

                    skipIt = false;
                    break;
                }

                if (skipIt)
                    continue;
            }

            // distance filter
            var relativePosition = transform.position;
            relativePosition.y = lowerLedge.GrabPosition.Value.y;

            var distance = Vector3.Distance(relativePosition, lowerLedge.GrabPosition.Value);
            if (distance > shortestDistance)
                continue;

            nearestLedge = lowerLedge;
            shortestDistance = distance;
        }

        return nearestLedge;
    }

    private void PushToLedge(GroundedLedgeBehaviour.ILowerLedge nearestLedge, ref Vector3 velocity)
    {
        var currentLedgeSpeed = Vector3.Project(velocity, nearestLedge.GrabPosition.PerpendicularGrabDirection);
        velocity -= currentLedgeSpeed;

        var fallDirection = nearestLedge.IsGrounded ?
                    nearestLedge.GrabPosition.PerpendicularGrabDirection : -nearestLedge.GrabPosition.PerpendicularGrabDirection;

        velocity += fallDirection * PushToLedgeSpeed;
    }

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
