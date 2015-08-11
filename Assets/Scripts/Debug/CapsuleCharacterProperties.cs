using UnityEngine;
using System.Collections;
using Common;

public class CapsuleCharacterProperties : MonoBehaviour, ICharacterProperties
{
    [Tooltip("The maximum step height the character can climb")]
    public float MaximumStepHeight = 0.3F;

    private CapsuleCollider _capsule;

    protected void Awake()
    {
        _capsule = GetComponent<CapsuleCollider>();
    }

    public float Height
    {
        get 
        { 
            return _capsule != null ? _capsule.height : 0F;
        }
    }

    public float Radius
    {
        get 
        {
            return _capsule != null ? _capsule.radius : 0F;
        }
    }

    public float StepOffset
    {
        get 
        {
            return MaximumStepHeight;
        }
    }
}
