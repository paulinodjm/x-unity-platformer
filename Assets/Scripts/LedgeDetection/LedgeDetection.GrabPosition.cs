using UnityEngine;

public static partial class LedgeUtils 
{
    public interface IGrabPosition
    {
        /// <summary>
        /// Returns the grab position
        /// </summary>
        Vector3 Position
        {
            get;
        }
    }

    private class GrabPosition : IGrabPosition
    {
        public Vector3 Position
        {
            get;
            set;
        }
    }
}
