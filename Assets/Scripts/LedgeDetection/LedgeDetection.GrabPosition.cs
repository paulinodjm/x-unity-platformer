using UnityEngine;

public static partial class LedgeUtils
{
    public interface IGrabPosition
    {
        /// <summary>
        /// Returns the grab position
        /// </summary>
        Vector3 Value
        {
            get;
        }

        /// <summary>
        /// Returns the position from which the calculation was made
        /// </summary>
        Vector3 FromPosition
        {
            get;
        }

        /// <summary>
        /// Check if the character can stand up on top of the ledge by performing a vertical capsule swipe.
        /// </summary>
        /// <param name="side">The ledge side from which perform the swipe</param>
        /// <param name="capsuleRadius">The character capsule radius</param>
        /// <param name="capsuleHeight">The character capsule height</param>
        /// <param name="ledgeDistance">The distance between the character feet and the grab position</param>
        /// <param name="swipeHeight">The capsule swipe height</param>
        /// <param name="collisionMask">The collision layer mask</param>
        /// <param name="errorMargin">A margin added between the ground and the character capsule to prevent collision check errors. 
        /// This value is reported on the result; keep it as small as possible.</param>
        /// <returns>The climb position, if any; null if the character can't stand here</returns>
        IClimbPosition CheckClimbPosition(SideStyle side, float capsuleRadius, float capsuleHeight, float ledgeDistance, float swipeHeight, LayerMask collisionMask, float errorMargin);

        /// <summary>
        /// Check if the character can fall of the ledge by performing a vertical capsule swipe.
        /// </summary>
        /// <param name="side">The ledge side from which perform the swipe</param>
        /// <param name="capsuleRadius">The character capsule radius</param>
        /// <param name="capsuleHeight">The character capsule height</param>
        /// <param name="ledgeDistance">The distance between the character capsule and the grab position</param>
        /// <param name="swipeHeight">The capsule swipe height</param>
        /// <param name="collisionMask">The collision layer mask</param>
        /// <param name="errorMargin">A margin added between the ground and the character capsule to prevent collision check errors. 
        /// This value is reported on the result; keep it as small as possible.</param>
        /// <returns>The fall position, if any; null if the character can't fall of the ledge</returns>
        IFallPosition CheckFallPosition(SideStyle side, float capsuleRadius, float capsuleHeight, float ledgeDistance, float swipeHeight, LayerMask collisionMask, float errorMargin);
    }

    private class GrabPosition : IGrabPosition
    {
        public Vector3 Value
        {
            get;
            set;
        }

        public Vector3 FromPosition
        {
            get;
            set;
        }

        /// <summary>
        /// The grab direction to go to the grab position, ignoring the y axis, and perpendicular to the edge.
        /// </summary>
        public Vector3 PerpendicularGrabDirection
        {
            get;
            set;
        }

        public IClimbPosition CheckClimbPosition(SideStyle side, float capsuleRadius, float capsuleHeight, float ledgeDistance, float swipeHeight, LayerMask collisionMask, float errorMargin)
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

            IClimbPosition result;
            if (Physics.CheckCapsule(bottomPosition, topPosition, capsuleRadius, collisionMask))
            {
                Debug.DrawLine(bottomPosition, topPosition, Color.red);
                result = null;
            }
            else
            {
                result = new ClimbPosition()
                {
                    Value = climbPosition,
                    GrabPosition = this,
                };
            }

            return result;
        }

        public IFallPosition CheckFallPosition(SideStyle side, float capsuleRadius, float capsuleHeight, float ledgeDistance, float swipeHeight, LayerMask collisionMask, float errorMargin)
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

            return new FallPosition()
            {
                Value = fallPosition,

                GrabPosition = this,
            };
        }
    }
}
