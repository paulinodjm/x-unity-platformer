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
            // délimite le rebord
            //
            var ledgeStart = ledge.transform.position;
            var ledgeEnd = ledgeStart + (ledge.transform.right * ledge.transform.lossyScale.x);

            //
            // calcule le point d'accroche
            //
            // <- à hauteur du joueur
            vector = ledgeStart - transform.position;
            grabPosition = Vector3.Project(vector, ledge.transform.forward) + transform.position;
            var oldGrabPosition = grabPosition;

            // <- à hauteur du rebord
            //vector = grabPosition - ledge.transform.position;
            var ledgeEndVertical = ledgeEnd;
            ledgeEndVertical.y = ledgeStart.y;
            var pct = Mathf.InverseLerp(0, Vector3.Distance(ledgeStart, ledgeEndVertical), Vector3.Distance(ledgeStart, grabPosition));
            grabPosition.y = Mathf.Lerp(ledgeStart.y, ledgeEnd.y, pct);

            var playerPosition = transform.position;
            playerPosition.y = grabPosition.y;

            //
            // valide le point d'accroche
            //
            bool grabPositionOk;

            if ((grabPosition - ledgeStart).normalized == -ledge.transform.right)
            {
                grabPositionOk = false;
            }
            else
            {
                var distanceFromLedgeStart = (grabPosition - ledgeStart).magnitude;
                grabPositionOk = distanceFromLedgeStart <= ledge.transform.lossyScale.x;
            }
            if (!grabPositionOk)
                continue;

            //
            // gère les extrémités de rebords (attention aux pentes!)
            //
            Vector3 safeGrabPosition;
            if (ledge.transform.lossyScale.x <= (_characterController.radius * 2))
            {
                safeGrabPosition = ledgeStart + (ledge.transform.right * _characterController.radius);
            }
            else if ((grabPosition - ledgeStart).magnitude < _characterController.radius)
            {
                safeGrabPosition = ledgeStart + (ledge.transform.right * _characterController.radius);
            }
            else if ((grabPosition - ledgeEnd).magnitude < _characterController.radius)
            {
                safeGrabPosition = ledgeEnd - (ledge.transform.right * _characterController.radius);
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
