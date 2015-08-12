using UnityEngine;

public static partial class LedgeUtils 
{
    public interface IClimbPosition
    {
        Vector3 Value
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
    }
}
