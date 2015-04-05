using UnityEngine;
using System.Collections;

public class PlayerAnimationController : MonoBehaviour
{
    #region Inspector

    public string SpeedParameter = "Speed";

    public string GroundedParameter = "Grounded";

    [Range(0, 1)]
    public float RotationLerpFactor = 0.5F;

    #endregion

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateAnimation(Vector3 velocity, bool grounded)
    {
        _animator.SetBool(GroundedParameter, grounded);

        velocity.y = 0F;
        _animator.SetFloat(SpeedParameter, velocity.magnitude);
        if (velocity.magnitude != 0F)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velocity), RotationLerpFactor);
        }
    }
}
