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

    /// <summary>
    /// Returns the positions the character can reach using the legdes.
    /// </summary>
    public List<Vector3> TargetPositions { get; private set; }

    private CharacterController _characterController;

    public LedgeSensor()
    {
        TargetPositions = new List<Vector3>();
    }

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

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = new Color(1F, 0F, 0F, 0.5F);

        foreach (var position in TargetPositions)
        {
            var drawPosition = position;
            drawPosition.y += _characterController.radius;
            Gizmos.DrawSphere(drawPosition, _characterController.radius);
        }
    }

    void Update()
    {
        UpperLedges.Clear();
        LowerLedges.Clear();
        TargetPositions.Clear();

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

            Vector3 vector, grabPosition;

            //
            // calcule le point d'accroche
            //
            // <- à hauteur du joueur
            vector = ledge.Start - transform.position;
            grabPosition = Vector3.Project(vector, ledge.transform.forward) + transform.position;
            var oldGrabPosition = grabPosition;

            // <- à hauteur du rebord
            //vector = grabPosition - ledge.transform.position;
            var ledgeEndVertical = ledge.End;
            ledgeEndVertical.y = ledge.Start.y;
            var pct = Mathf.InverseLerp(0, Vector3.Distance(ledge.Start, ledgeEndVertical), Vector3.Distance(ledge.Start, grabPosition));
            grabPosition.y = Mathf.Lerp(ledge.Start.y, ledge.End.y, pct);

            var playerPosition = transform.position;
            playerPosition.y = grabPosition.y;

            //
            // valide le point d'accroche
            //
            bool grabPositionOk;

            if ((grabPosition - ledge.Start).normalized == -ledge.Direction)
            {
                grabPositionOk = false;
            }
            else
            {
                var distanceFromLedgeStart = (grabPosition - ledge.Start).magnitude;
                grabPositionOk = distanceFromLedgeStart <= ledge.Length;
            }
            if (!grabPositionOk)
                continue;

            //
            // gère les extrémités de rebords (attention aux pentes!)
            //
            Vector3 safeGrabPosition;
            if (ledge.Length <= (_characterController.radius * 2))
            {
                safeGrabPosition = ledge.Start + (ledge.Direction * _characterController.radius);
            }
            else if ((grabPosition - ledge.Start).magnitude < _characterController.radius)
            {
                safeGrabPosition = ledge.Start + (ledge.Direction * _characterController.radius);
            }
            else if ((grabPosition - ledge.End).magnitude < _characterController.radius)
            {
                safeGrabPosition = ledge.End - (ledge.Direction * _characterController.radius);
            }
            else
            {
                safeGrabPosition = grabPosition;
            }

            Debug.DrawLine(playerPosition, grabPosition, Color.red);

            var nextPosition = safeGrabPosition + (grabPosition - playerPosition).normalized * _characterController.radius;
            TargetPositions.Add(nextPosition);
        }
    }
}
