using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputController))]
public class PlayerController : MonoBehaviour
{
    #region Inspector

    [Tooltip("The player's point of view")]
    public GameObject Pov;

    [Tooltip("Walk parameters")]
    public GroundedMovementsInfo WalkParameters = new GroundedMovementsInfo()
    {
        Speed = 5F
    };

    #endregion

    private CharacterController _characterController;

    private InputController _inputController;

    /// <summary>
    /// Returns the current player point of view.
    /// </summary>
    /// <returns>The point of view</returns>
    public Transform GetPovTransform()
    {
        return (Pov != null) ? Pov.transform : transform;
    }

    /// <summary>
    /// Determines the movement axis when grounded.
    /// </summary>
    /// <param name="move">The move axis</param>
    /// <param name="strafe">The strafe axis</param>
    public void GetGroundAxis(out Vector3 move, out Vector3 strafe)
    {
        var pov = GetPovTransform();

        move = pov.forward;
        move.y = 0F;
        move.Normalize();

        strafe = pov.right;
        strafe.y = 0F;
        strafe.Normalize();
    }

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _inputController = GetComponent<InputController>();
    }
	
	void Update () 
    {
        Vector3 forward, strafe;
        GetGroundAxis(out forward, out strafe);

        forward *= _inputController.Forward;
        strafe *= _inputController.Strafe;
        
        var moveDirection = forward + strafe;
        if (moveDirection.magnitude > 1F)
        {
            moveDirection.Normalize();
        }

        var move = moveDirection * WalkParameters.Speed * Time.deltaTime;
        move.y = -5;
        _characterController.Move(move);
	}

    [Serializable]
    public struct GroundedMovementsInfo
    {
        [Tooltip("The speed when grounded")]
        public float Speed;
    }
}
