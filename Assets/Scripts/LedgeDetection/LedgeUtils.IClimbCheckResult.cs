using UnityEngine;

public static partial class LedgeUtils 
{
    public interface IClimbCheckResult
    {
        /// <summary>
        /// Returns the climb position value
        /// </summary>
        Vector3 Value
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
}
