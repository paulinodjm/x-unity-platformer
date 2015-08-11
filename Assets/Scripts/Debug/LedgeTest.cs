using UnityEngine;
using System.Collections;

public class LedgeTest : MonoBehaviour
{
    public Color LedgeColor = Color.black;
    
    private LedgeSensor _ledgeSensor;

    protected void Start()
    {
        _ledgeSensor = GetComponent<LedgeSensor>();
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
        }
    }
}
