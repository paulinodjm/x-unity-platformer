using UnityEngine;
using System.Collections;
using Common;

public class LedgeTest : MonoBehaviour
{
    [Header("Ledge")]
    public Color LowerLedgeColor = Color.black;
    public Color CrossedLedgeColor = Color.green;
    public Color UpperLedgeColor = Color.red;

    [Header("General")]
    public LayerMask CollisionLayer;
    public float ErrorMargin = .1F;

    [Header("Climb")]
    public Color ClimbPositionColor = Color.red;
    public float UpperLedgeDistance = .3F;
    public float StepHeight = .3F;

    [Header("Fall")]
    public Color FallPositionColor = Color.black;
    public float LowerLedgeDistance = .1F;
    public float FallHeight = 2F;

    private LedgeSensor _ledgeSensor;
    private ICharacterProperties _character;

    protected void Start()
    {
        _ledgeSensor = GetComponent<LedgeSensor>();
        _character = GetComponent<ICharacterProperties>();
    }

    protected void OnDrawGizmosSelected()
    {
        if (_ledgeSensor == null)
            return;

        Gizmos.matrix = Matrix4x4.identity;

        foreach (var ledge in _ledgeSensor.Ledges)
        {
            var grabPosition = ledge.CalcGrabPosition(transform.position, _character.Radius + LowerLedgeDistance);
            var isUpperLedge = grabPosition.Value.y >= transform.position.y + _character.StepOffset;

            var isCrossed = grabPosition.LedgeDistance <= _character.Radius;

            Gizmos.color = isUpperLedge ?  UpperLedgeColor : (isCrossed ? CrossedLedgeColor : LowerLedgeColor);
            Gizmos.DrawLine(ledge.Start, ledge.End);
            Gizmos.DrawSphere(grabPosition.Value, .1F);

            if (isUpperLedge)
            {
                var climbPosition = grabPosition.CheckClimbPosition(LedgeUtils.SideStyle.Far, _character.Radius, _character.Height, UpperLedgeDistance, StepHeight, CollisionLayer, ErrorMargin);
                if (climbPosition != null)
                {
                    var spherePosition = climbPosition.Value;
                    spherePosition.y += _character.Radius;

                    Gizmos.color = ClimbPositionColor;
                    Gizmos.DrawSphere(spherePosition, _character.Radius);
                }
            }
            else
            {
                var fallPosition = grabPosition.CheckFallPosition(LedgeUtils.SideStyle.Far, _character.Radius, _character.Height, LowerLedgeDistance, FallHeight, CollisionLayer, ErrorMargin);
                if (fallPosition != null)
                {
                    var spherePosition = fallPosition.Value;
                    spherePosition.y += _character.Radius;

                    Gizmos.color = FallPositionColor;
                    Gizmos.DrawSphere(spherePosition, _character.Radius);
                }
            }
        }
    }
}
