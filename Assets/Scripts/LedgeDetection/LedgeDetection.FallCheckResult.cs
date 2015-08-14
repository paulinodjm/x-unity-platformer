using UnityEngine;

public static partial class LedgeUtils 
{
    private class FallCheckResult : IFallCheckResult
    {
        public Vector3 Value
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
