using UnityEngine;

public static partial class LedgeUtils 
{
    public interface IFallCheckResult
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
}
