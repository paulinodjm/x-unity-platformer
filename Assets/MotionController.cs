using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class MotionController : MonoBehaviour 
{
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _animator.SetBool("Walk", Input.GetAxis("Vertical") > 0F);
    }
}
