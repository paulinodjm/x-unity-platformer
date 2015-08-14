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
        /// Returns the distance between the FromPosition and the nearest point of the ledge.
        /// </summary>
        float LedgeDistance
        {
            get;
        }

        /// <summary>
        /// The grab direction to go to the grab position, ignoring the y axis, and perpendicular to the ledge.
        /// </summary>
        Vector3 PerpendicularGrabDirection
        {
            get;
        }

        /// <summary>
        /// The distance to go to the grab position, ignoring the y axis, and perpendicular to the ledge
        /// </summary>
        float PerpendicularGrabDistance
        {
            get;
        }

        /// <summary>
        /// Tell whether or not the grab position is in front of the player
        /// </summary>
        bool IsInFront
        {
            get;
        }

        /// <summary>
        /// Returns the ledge
        /// </summary>
        Ledge Ledge
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
        /// The minimum margin asked between the grab position and the ledge ends
        /// </summary>
        float Margin
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
}
