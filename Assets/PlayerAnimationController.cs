using UnityEngine;
using System.Collections;

public class PlayerAnimationController : MonoBehaviour
{
    #region Inspector

    public string SpeedParameter = "Speed";

    public string GroundedParameter = "Grounded";

    public string ClimbOnDeepState = "ClimbOnDeep";

    public string ClimbOnState = "ClimbOn";

    public string ClimbOffState = "ClimbOff";

    public string LedgeStopParameter = "LedgeStop";

    public float TransitionDuration = 0F;

    [Range(0, 1)]
    public float RotationLerpFactor = 0.5F;

    #endregion

    private Animator _animator;
    private CharacterController _character;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _character = GetComponent<CharacterController>();
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

    public void SetLedgeAnimation(Vector3 direction, float height)
    {
        string state;
        if (height >= 0F)
        {
            state = (height > _character.radius / 2) ? ClimbOnDeepState : ClimbOnState;
        }
        else
        {
            state = ClimbOffState;
        }

        _animator.CrossFade(state, TransitionDuration);
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void SetLedgeStopAnimation()
    {
        _animator.SetTrigger(LedgeStopParameter);
    }
}
