using UnityEngine;
using System.Collections.Generic;

public class LedgeSensor : MonoBehaviour 
{
    /// <summary>
    /// Returns the ledges near the character.
    /// </summary>
    public List<Ledge> Ledges { get; private set; }

    /// <summary>
    /// Returns the ledges the character can climb on.
    /// </summary>
    public List<Ledge> UpperLedges { get; private set; }

    /// <summary>
    /// Returns the ledges the character can climb down.
    /// </summary>
    public List<Ledge> LowerLedges { get; private set; }

    private CharacterController _characterController;

    void Awake()
    {
        Ledges = new List<Ledge>();
        UpperLedges = new List<Ledge>();
        LowerLedges = new List<Ledge>();
        _characterController = GetComponent<CharacterController>();
    }

    void OnTriggerEnter(Collider collider)
    {
        var ledge = collider.gameObject.GetComponentInParent<Ledge>();
        if (ledge == null) return;

        Ledges.Add(ledge);
    }

    void OnTriggerExit(Collider collider)
    {
        var ledge = collider.gameObject.GetComponentInParent<Ledge>();
        if (ledge == null) return;

        Ledges.Remove(ledge);
    }

    void Update()
    {
        UpperLedges.Clear();
        LowerLedges.Clear();

        foreach (var ledge in Ledges)
        {
            if (ledge.transform.position.y < transform.position.y)
            {
                LowerLedges.Add(ledge);
            }
            else
            {
                UpperLedges.Add(ledge);
            }
        }
    }
}
