using UnityEngine;
using System.Collections;

public class HeadController : MonoBehaviour
{
    public float RotationSpeedIn;
    public float RotationSpeedOut;
    public float MaxAngle;
    public Transform Bone;
    public Transform PointOfView;

    void LateUpdate()
    {
        Bone.rotation = CalcHeadRotation();
    }

    private Quaternion CalcHeadRotation()
    {
        if (PointOfView == null) return Bone.rotation;

        var desiredRotation = Quaternion.LookRotation(PointOfView.forward, PointOfView.right);
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
