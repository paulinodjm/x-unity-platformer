using UnityEngine;
using System.Collections;

public class HeadController : MonoBehaviour
{
    public float RotationSpeedIn;
    public float RotationSpeedOut;
    public float MaxAngle;
    public Transform Bone;

    private Transform _camera;

    void Start()
    {
        _camera = GetComponent<PlayerController>().GetPovTransform();
    }

    void LateUpdate()
    {
        Bone.rotation = CalcHeadRotation();
    }

    private Quaternion CalcHeadRotation()
    {
        if (_camera == null) return Bone.rotation;

        var desiredRotation = Quaternion.LookRotation(_camera.forward, _camera.right);
        var playerRotation = Quaternion.LookRotation(transform.forward, transform.right);
        var rotation = Quaternion.identity;
        var deltaAngle = Mathf.DeltaAngle(desiredRotation.eulerAngles.y, playerRotation.eulerAngles.y);

        if (deltaAngle > MaxAngle || deltaAngle < -MaxAngle)
        {
            rotation = Quaternion.Lerp(Bone.rotation, playerRotation, RotationSpeedOut);
        }
        else
        {
            rotation = Quaternion.Lerp(Bone.rotation, desiredRotation, RotationSpeedIn);
        }
        return rotation;
    }
}
