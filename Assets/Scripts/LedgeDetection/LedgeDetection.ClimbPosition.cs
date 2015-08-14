using UnityEngine;

public static partial class LedgeUtils 
{
    public interface IClimbPosition
    {
        /// <summary>
        /// Returns the climb position value
        /// </summary>
        Vector3 Value
        {
            get;
        }

        /// <summary>
        /// Returns a value indicatif whether the grab position is in front of the player, at worst with an offset less than the given margin.
        /// </summary>
        bool IsInFront
        {
            get;
        }

        /// <summary>
        /// Returns the distance between the character position and the ledge, perpendicular to the ledge
        /// </summary>
        float PerpendicularGrabDistance
        {
            get;
        }

        /// <summary>
        /// Returns the direction between the character position and the ledge, perpendicular to toe ledge
        /// </summary>
        Vector3 PerpendicularGrabDirection
        {
            get;
        }

        /// <summary>
        /// Returns the grab position from which the calculation has been made
        /// </summary>
        IGrabPosition GrabPosition
        {
            get;
        }
    }

    private class ClimbPosition : IClimbPosition
    {
        public Vector3 Value
        {
            get;
            set;
        }

        public bool IsInFront
        {
            get;
            set;
        }

        public float PerpendicularGrabDistance
        {
            get;
            set;
        }

        public Vector3 PerpendicularGrabDirection
        {
            get;
            set;
        }

        public IGrabPosition GrabPosition
        {
            get;
            set;
        }
    }
}
