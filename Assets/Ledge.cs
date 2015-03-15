using UnityEngine;
using System.Collections;

public class Ledge : MonoBehaviour 
{
    private static readonly Color _color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5F);

    private const float _borderSize = 0.05F;

    private const float _width = _borderSize * 2F;

    private const float _height = 0.2F;
         
    void OnDrawGizmos()
    {
        Gizmos.color = _color;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        var xOffset = transform.lossyScale.x / 2;
        var center = new Vector3(xOffset, -_borderSize, 0);

        var size = new Vector3(transform.lossyScale.x, _height, _width);

        Gizmos.DrawCube(center, size);
    }
}
