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

    public List<Ledge> UpperLedges;
    public List<Ledge> LowerLedges;

    #endregion

    private LedgeSensor _ledgeSensor;
    private ICharacterProperties _character;

    protected void Start()
    {
        _ledgeSensor = GetComponent<LedgeSensor>();
        _character = GetComponent<ICharacterProperties>();
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
                Debug.DrawRay(grabPosition.Value, Vector3.up, LowerLedgeColor);
                LowerLedges.Add(ledge);
            }
            else
            {
                // discard this ledge
            }
        }
    }

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

        Debug.DrawRay(climbPosition.Value, Vector3.up, UpperLedgeColor);

        UpperLedges.Add(grabPosition.Ledge);
    }
}
