using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class MotionController : MonoBehaviour
{
    private Animator _animator;

    private float _lastDirection;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        var move = new Vector3(Input.GetAxis("Horizontal"), 0F, Input.GetAxis("Vertical")).normalized;
        var direction = move.magnitude > 0F ? Mathf.DeltaAngle(transform.rotation.eulerAngles.y, Quaternion.LookRotation(move).eulerAngles.y) : _lastDirection;
        _lastDirection = direction;

        if (move.magnitude > 0F)
        {
            var rotationRate = _animator.GetFloat("RotationRate") * 0;
            print(rotationRate);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(move), rotationRate);
        }

        _animator.SetBool("Walk", move.magnitude > 0F);
        _animator.SetFloat("Direction", direction);
    }

    void OnGUI()
    {
        GUILayout.Label(_lastDirection.ToString());
    }
}
