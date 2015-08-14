﻿using UnityEngine;
using System.Collections.Generic;
using Common;

[RequireComponent(typeof(LedgeSensor))]
[RequireComponent(typeof(ICharacterProperties))]
public class GroundedLedgeBehaviour : MonoBehaviour
{
    #region Inspector

    [Tooltip("The distance kept between the character and the ledge, when he falls or grab the ledge; prevents collision check errors")]
    public float WallMargin = 0.02F;

    [Tooltip("The delta height allowed between the grab position and the actual target feet position, when the character climb on a ledge")]
    public float ClimbMargin = 0.2F;

    [Tooltip("The distance kept between the character and the ground; prevents collision check error. Keep it as small as possible")]
    public float GroundMargin = 0.02F;

    [Tooltip("The mask used for the collision checks; Tell wich layers are \"solid\" or not")]
    public LayerMask CollisionMask;

    [Tooltip("The distance kept between a ledge and the character feet when he stands on top of the ledge")]
    public float FallDistance = 0.3F;

    [Tooltip("The vertical distance to check when capsule casting the fall position")]
    public float FallHeight = 2F;

    public Color UpperLedgeColor = Color.blue;
    public Color LowerLedgeColor = Color.black;

    #endregion

    /// <summary>
    /// Returns the available upper ledges
    /// </summary>
    public List<IUpperLedge> UpperLedges
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns the available lower ledges
    /// </summary>
    public List<ILowerLedge> LowerLedges
    {
        get;
        private set;
    }

    private LedgeSensor _ledgeSensor;
    private ICharacterProperties _character;

    protected void Start()
    {
        _ledgeSensor = GetComponent<LedgeSensor>();
        _character = GetComponent<ICharacterProperties>();

        UpperLedges = new List<IUpperLedge>();
        LowerLedges = new List<ILowerLedge>();
    }

    protected void Update()
    {
        UpperLedges.Clear();
        LowerLedges.Clear();

        foreach (var ledge in _ledgeSensor.Ledges)
        {
            var grabPosition = ledge.CalcGrabPosition(transform.position, _character.Radius + WallMargin);
            var deltaHeight = grabPosition.Value.y - transform.position.y;

            if (deltaHeight > _character.StepOffset)
            {
                HandleUpperLedge(grabPosition);
            }
            else if (deltaHeight >= -_character.StepOffset)
            {
                HandleLowerLedge(grabPosition);
            }
            else
            {
                // discard this ledge
            }
        }
    }

    /// <summary>
    /// Handle the upper ledge detection
    /// </summary>
    /// <param name="grabPosition">The grab position</param>
    private void HandleUpperLedge(LedgeUtils.IGrabPosition grabPosition)
    {
        var climbPosition = grabPosition.CheckClimbPosition(LedgeUtils.SideStyle.Far, _character.Radius, _character.Height, FallDistance, ClimbMargin, CollisionMask, GroundMargin);
        if (climbPosition == null)
            return;

        var fallPosition = grabPosition.CheckFallPosition(LedgeUtils.SideStyle.Near, _character.Radius, _character.Height, _character.Radius + WallMargin, FallHeight, CollisionMask, GroundMargin);
        if (fallPosition == null)
            return;

        // discard steps
        var deltaHeight = climbPosition.Value.y - fallPosition.Value.y;
        if (deltaHeight <= _character.StepOffset)
            return;

        if (climbPosition.GrabPosition.IsInFront)
        {
            Debug.DrawRay(climbPosition.Value, Vector3.up, UpperLedgeColor);
            Debug.DrawRay(grabPosition.Value, -climbPosition.GrabPosition.PerpendicularGrabDirection * climbPosition.GrabPosition.PerpendicularGrabDistance, UpperLedgeColor);
        }

        UpperLedges.Add(
            new UpperLedge()
            {
                GrabPosition = grabPosition,
                TargetPosition = climbPosition.Value,
            }
        );
    }

    /// <summary>
    /// Handle the lower ledge detection
    /// </summary>
    /// <param name="grabPosition">The grab position</param>
    private void HandleLowerLedge(LedgeUtils.IGrabPosition grabPosition)
    {
        // params
        var groundPosition = grabPosition.Value.y + GroundMargin;

        // results
        Vector3? fallPosition;
        LedgeUtils.SideStyle upperSide;

        // computations
        var farFallPosition = grabPosition.CheckFallPosition(LedgeUtils.SideStyle.Far, _character.Radius, _character.Height, WallMargin, FallHeight, CollisionMask, GroundMargin);
        if (farFallPosition != null)
        {
            if (groundPosition - farFallPosition.Value.y > _character.StepOffset)
            {
                fallPosition = farFallPosition.Value;
                upperSide = LedgeUtils.SideStyle.Near;
            }
            else
            {
                var nearFallPosition = grabPosition.CheckFallPosition(LedgeUtils.SideStyle.Near, _character.Radius, _character.Height, WallMargin, FallHeight, CollisionMask, GroundMargin);
                if (nearFallPosition != null)
                {
                    if (groundPosition - nearFallPosition.Value.y > _character.StepOffset)
                    {
                        fallPosition = nearFallPosition.Value;
                        upperSide = LedgeUtils.SideStyle.Far;
                    }
                    else
                    {
                        if (Mathf.Abs(nearFallPosition.Value.y - farFallPosition.Value.y) > _character.StepOffset)
                        {
                            if (nearFallPosition.Value.y < farFallPosition.Value.y)
                            {
                                fallPosition = nearFallPosition.Value;
                                upperSide = LedgeUtils.SideStyle.Far;
                            }
                            else
                            {
                                fallPosition = farFallPosition.Value;
                                upperSide = LedgeUtils.SideStyle.Near;
                            }
                        }
                        else
                        {
                            // the difference between the two fall position is not big enough. Don't retains the ledge
                            return;
                        }
                    }
                }
                else
                {
                    fallPosition = null;
                    upperSide = LedgeUtils.SideStyle.Far;
                }
            }
        }
        else
        {
            var nearFallPosition = grabPosition.CheckFallPosition(LedgeUtils.SideStyle.Near, _character.Radius, _character.Height, WallMargin, FallHeight, CollisionMask, GroundMargin);
            if (nearFallPosition != null)
            {
                if (groundPosition - nearFallPosition.Value.y > _character.StepOffset)
                {
                    fallPosition = nearFallPosition.Value;
                    upperSide = LedgeUtils.SideStyle.Far;
                }
                else
                {
                    fallPosition = null;
                    upperSide = LedgeUtils.SideStyle.Near;
                }
            }
            else
            {
                // both side are blocked. Don't retains the ledge
                return;
            }
        }

        var climbPosition = grabPosition.CheckClimbPosition(upperSide, _character.Radius, _character.Height, FallDistance, ClimbMargin, CollisionMask, GroundMargin);
        Vector3? upPosition;
        if (climbPosition != null)
        {
            upPosition = climbPosition.Value;
        }
        else
        {
            upPosition = null;
        }

        if (fallPosition != null)
        {
            Debug.DrawRay(fallPosition.Value, Vector3.up, LowerLedgeColor);
        }

        if (upPosition != null)
        {
            Debug.DrawRay(upPosition.Value, Vector3.up, UpperLedgeColor);
        }

        LowerLedges.Add(
            new LowerLedge()
            {
                GrabPosition = grabPosition,
                DownPosition = fallPosition,
                UpPosition = upPosition,
                IsGrounded = (upperSide == LedgeUtils.SideStyle.Near)
            }
        );
    }

    public interface IUpperLedge
    {
        /// <summary>
        /// Returns the attached grab position
        /// </summary>
        LedgeUtils.IGrabPosition GrabPosition
        {
            get;
        }

        /// <summary>
        /// Returns the target position, on top of the ledge
        /// </summary>
        Vector3 TargetPosition
        {
            get;
        }
    }

    public interface ILowerLedge
    {
        /// <summary>
        /// Returns the attached grab position
        /// </summary>
        LedgeUtils.IGrabPosition GrabPosition
        {
            get;
        }

        /// <summary>
        /// Returns the character position down the ledge, if any
        /// </summary>
        Vector3? DownPosition
        {
            get;
        }

        /// <summary>
        /// Returns the character position on top of the ledge, if any
        /// </summary>
        Vector3? UpPosition
        {
            get;
        }

        /// <summary>
        /// Tells weahter the character stands on the grounded side of the ledge or not
        /// </summary>
        bool IsGrounded
        {
            get;
        }
    }

    private class UpperLedge : IUpperLedge
    {
        public LedgeUtils.IGrabPosition GrabPosition
        {
            get;
            set;
        }

        public Vector3 TargetPosition
        {
            get;
            set;
        }
    }

    private class LowerLedge : ILowerLedge
    {
        public LedgeUtils.IGrabPosition GrabPosition
        {
            get;
            set;
        }

        public Vector3? DownPosition
        {
            get;
            set;
        }

        public Vector3? UpPosition
        {
            get;
            set;
        }

        public bool IsGrounded
        {
            get;
            set;
        }
    }
}
