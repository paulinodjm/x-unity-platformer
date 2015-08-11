using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects and keep an eye on the ledges arount the character
/// </summary>
public class LedgeSensor : MonoBehaviour
{
    /// <summary>
    /// Returns the ledges near the character.
    /// </summary>
    public List<Ledge> Ledges { get; private set; }

    protected void Start()
    {
        Ledges = new List<Ledge>();
    }

    protected void OnTriggerEnter(Collider collider)
    {
        var ledge = collider.gameObject.GetComponentInParent<Ledge>();
        if (ledge == null) return;

        Ledges.Add(ledge);
    }

    protected void OnTriggerExit(Collider collider)
    {
        var ledge = collider.gameObject.GetComponentInParent<Ledge>();
        if (ledge == null) return;

        Ledges.Remove(ledge);
    }
}
