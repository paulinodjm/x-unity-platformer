using UnityEngine;

public static partial class LedgeUtils 
{
    /// <summary>
    /// Calcule la position la plus proche sur le rebord.
    /// </summary>
    /// <param name="ledge">Le rebord</param>
    /// <param name="position">La position à partir de laquelle le calcul est fait</param>
    /// <param name="margin">La distance minimum à garder entre le point et chaque extrémité du rebord</param>
    /// <returns>La position la plus proche sur le rebord</returns>
    public static IGrabPosition CalcGrabPosition(this Ledge ledge, Vector3 position, float margin = 0F)
    {
        var relativePosition = position - ledge.Start;
        relativePosition.y = 0;

        var rawGrabPosition = Vector3.Project(-relativePosition, ledge.transform.forward) + relativePosition;

        var grabVector = rawGrabPosition - relativePosition;

        var perpendicularGrabPosition = grabVector.normalized;
        var perpendicularGrabDistance = grabVector.magnitude;

        if (rawGrabPosition.normalized == -ledge.FlatDirection)
        {
            rawGrabPosition = Vector3.zero;
        }
        else if (rawGrabPosition.magnitude > ledge.FlatLength)
        {
            rawGrabPosition = ledge.FlatEnd - ledge.Start;
        }
        else
        {
            // valid
        }

        Vector3 safeGrabPosition;
        if (ledge.FlatLength <= margin * 2F)
        {
            safeGrabPosition = ledge.Start + (ledge.FlatDirection * (ledge.FlatLength / 2F));
        }
        else if (rawGrabPosition.magnitude < margin)
        {
            safeGrabPosition = ledge.Start + (ledge.FlatDirection * margin);
        }
        else if (rawGrabPosition.magnitude > ledge.FlatLength - margin)
        {
            safeGrabPosition = ledge.FlatEnd - (ledge.FlatDirection * margin);
        }
        else
        {
            safeGrabPosition = ledge.Start + rawGrabPosition;
        }

        var normalizedGrabPosition = Mathf.InverseLerp(0, ledge.FlatLength, Vector3.Distance(ledge.Start, safeGrabPosition));
        var height = Mathf.Lerp(ledge.Start.y, ledge.End.y, normalizedGrabPosition);

        var grabPosition = safeGrabPosition;
        grabPosition.y = height;

        return new GrabPosition()
        {
            Position = grabPosition,
        };
    }
}
