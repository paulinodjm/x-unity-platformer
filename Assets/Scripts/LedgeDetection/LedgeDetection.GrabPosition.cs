﻿using UnityEngine;

public static partial class LedgeUtils
{
    private class GrabPosition : IGrabPosition
    {
        public Vector3 Value
        {
            get;
            set;
        }

        public float LedgeDistance
        {
            get;
            set;
        }

        public Ledge Ledge
        {
            get;
            set;
        }

        public Vector3 FromPosition
        {
            get;
            set;
        }

        public float Margin
        {
            get;
            set;
        }

        public Vector3 PerpendicularGrabDirection
        {
            get;
            set;
        }

        public float PerpendicularGrabDistance
        {
            get;
            set;
        }

        public bool IsInFront
        {
            get;
            set;
        }

        public IClimbCheckResult CheckClimbPosition(SideStyle side, float capsuleRadius, float capsuleHeight, float ledgeDistance, float swipeHeight, LayerMask collisionMask, float errorMargin)
        {
            var direction = (side == SideStyle.Far) ? PerpendicularGrabDirection : -PerpendicularGrabDirection;
            var footPosition = Value + (direction * ledgeDistance);

            // capsule cast

            var bottomPosition = footPosition;
            bottomPosition.y += errorMargin + swipeHeight + capsuleRadius;

            var topPosition = bottomPosition;
            topPosition.y += capsuleHeight - (2 * capsuleRadius);

            var climbPosition = footPosition;
            climbPosition.y += errorMargin;

            RaycastHit hitInfo;
            if (Physics.CapsuleCast(topPosition, bottomPosition, capsuleRadius, Vector3.down, out hitInfo, swipeHeight, collisionMask))
            {
                climbPosition.y = hitInfo.point.y + errorMargin;
            }

            // final capsule check

            bottomPosition = climbPosition;
            bottomPosition.y += capsuleRadius;

            topPosition = bottomPosition;
            topPosition.y += capsuleHeight - (2 * capsuleRadius);

            IClimbCheckResult result;
            if (Physics.CheckCapsule(bottomPosition, topPosition, capsuleRadius, collisionMask))
            {
                Debug.DrawLine(bottomPosition, topPosition, Color.red);
                result = null;
            }
            else
            {
                result = new ClimbCheckResult()
                {
                    Value = climbPosition,
                    GrabPosition = this,
                };
            }

            return result;
        }

        public IFallCheckResult CheckFallPosition(SideStyle side, float capsuleRadius, float capsuleHeight, float ledgeDistance, float swipeHeight, LayerMask collisionMask, float errorMargin)
        {
            var direction = (side == SideStyle.Far) ? PerpendicularGrabDirection : -PerpendicularGrabDirection;
            var footPosition = Value + (direction * (ledgeDistance + capsuleRadius));

            // first capsule check
            var bottomPosition = footPosition;
            bottomPosition.y += errorMargin + capsuleRadius;

            var topPosition = bottomPosition;
            topPosition.y += capsuleHeight - (2 * capsuleRadius);

            if (Physics.CheckCapsule(bottomPosition, topPosition, capsuleRadius, collisionMask))
            {
                Debug.DrawLine(bottomPosition, topPosition, Color.black);
                return null;
            }

            // capsule cast

            var fallPosition = footPosition;
            fallPosition.y += errorMargin;

            RaycastHit hitInfo;
            if (Physics.CapsuleCast(bottomPosition, topPosition, capsuleRadius, Vector3.down, out hitInfo, swipeHeight + errorMargin, collisionMask))
            {
                fallPosition.y = hitInfo.point.y + errorMargin;
            }
            else
            {
                fallPosition.y -= swipeHeight;
            }

            return new FallCheckResult()
            {
                Value = fallPosition,

                GrabPosition = this,
            };
        }
    }
}
