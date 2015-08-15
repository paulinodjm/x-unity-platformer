using UnityEngine;
using System.Collections;
using Common;

public class CharacterProperties : MonoBehaviour, ICharacterProperties
{
    private CharacterController _characterController;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public float Height
    {
        get 
        {
            return _characterController.height;
        }
    }

    public float Radius
    {
        get 
        {
            return _characterController.radius;
        }
    }

    public float StepOffset
    {
        get 
        {
            return _characterController.stepOffset;
        }
    }
}
