using UnityEngine;

public class Ledge : MonoBehaviour
{
    #region Gizmos parameters

    private static readonly Color _color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5F);

    private const float _borderSize = 0.05F;

    private const float _width = _borderSize * 2F;

    private const float _height = 0.2F;

    #endregion

    private const float _colliderRadius = 1.0F;

    void OnDrawGizmos()
    {
        Gizmos.color = _color;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        var xOffset = transform.lossyScale.x / 2;
        var center = new Vector3(xOffset, -_borderSize, 0);

        var size = new Vector3(transform.lossyScale.x, _height, _width);

        Gizmos.DrawCube(center, size);
    }

    void Awake()
    {
        SpawnCapsule();
    }

    /// <summary>
    /// Creates the capsule used for the ledge detection.
    /// </summary>
    private void SpawnCapsule()
    {
        var capsuleGameObject = new GameObject("Trigger");

        capsuleGameObject.transform.position = transform.position;
        capsuleGameObject.transform.rotation = transform.rotation;
        capsuleGameObject.layer = gameObject.layer;

        var capsule = capsuleGameObject.AddComponent<CapsuleCollider>();
        capsule.radius = _colliderRadius;
        capsule.direction = 0;
        capsule.height = transform.lossyScale.x + capsule.radius * 2F;
        capsule.center = new Vector3(transform.lossyScale.x / 2F, 0F, 0F);
        capsule.isTrigger = true;

        capsuleGameObject.transform.parent = transform;
    }
}
