using UnityEngine;
using System.Collections;
using Common;

public class LedgeTest : MonoBehaviour
{
    public Color LedgeColor = Color.black;
    
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

        Gizmos.color = LedgeColor;
        Gizmos.matrix = Matrix4x4.identity;

        foreach (var ledge in _ledgeSensor.Ledges)
        {
            Gizmos.DrawLine(ledge.Start, ledge.End);

            var grabPosition = ledge.CalcGrabPosition(transform.position, _character.Radius);
            Gizmos.DrawSphere(grabPosition.Position, .1F);
        }
    }
}
