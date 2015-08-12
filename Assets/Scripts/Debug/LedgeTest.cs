using UnityEngine;
using System.Collections;
using Common;

public class LedgeTest : MonoBehaviour
{
    public Color LowerLedgeColor = Color.black;
    public Color UpperLedgeColor = Color.red;
    
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
            var grabPosition = ledge.CalcGrabPosition(transform.position, _character.Radius);

            Gizmos.color = (grabPosition.Position.y < transform.position.y + _character.StepOffset) ? LowerLedgeColor : UpperLedgeColor;

            Gizmos.DrawLine(ledge.Start, ledge.End);
            Gizmos.DrawSphere(grabPosition.Position, .1F);
        }

        print(_ledgeSensor.Ledges.Count);
    }
}
