using UnityEngine;

public static partial class LedgeUtils 
{
    public interface IFallPosition
    {
        Vector3 Value
        {
            get;
        }

        IGrabPosition GrabPosition
        {
            get;
        }
    }

    private class FallPosition : IFallPosition
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
