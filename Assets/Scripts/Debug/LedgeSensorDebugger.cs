using UnityEngine;

public class LedgeSensorDebugger : MonoBehaviour 
{
    public Mesh PlayerMesh;
    public Vector3 MeshOffset;

    public float GrabSphereRadius = .3F;
    public Vector3 GrabArrowOffset;

    public Color LedgeColor = Color.black;
    public Color StepColor = Color.gray;

    public Color FromPositionColor = Color.green;
    public Color TargetPositionColor = Color.cyan;

    private LedgeSensor _ledgeSensor;

    protected void Awake()
    {
        _ledgeSensor = GetComponent<LedgeSensor>();
    }

    protected void OnDrawGizmosSelected()
    {
        if (_ledgeSensor == null)
            return;

        Gizmos.matrix = Matrix4x4.identity;

        foreach (var grab in _ledgeSensor.GrabInfos)
        {
            var ledgeColor = grab.IsStep ? StepColor : LedgeColor;

            Gizmos.color = ledgeColor;
            Gizmos.DrawLine(grab.Ledge.Start, grab.Ledge.End);
            Gizmos.DrawSphere(grab.GrabPosition, GrabSphereRadius);

            if (grab.HasFromPosition)
            {
                Gizmos.color = ledgeColor;
                Gizmos.DrawLine(grab.GrabPosition + GrabArrowOffset, grab.FromPosition);

                Gizmos.color = FromPositionColor;
                Gizmos.DrawMesh(PlayerMesh, grab.FromPosition + MeshOffset);
            }

            if (grab.HasTargetPosition)
            {
                Gizmos.color = ledgeColor;
                Gizmos.DrawLine(grab.GrabPosition + GrabArrowOffset, grab.TargetPosition);

                Gizmos.color = TargetPositionColor;
                Gizmos.DrawMesh(PlayerMesh, grab.TargetPosition + MeshOffset);
            }
        }
    }
}
