using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour
{
    #region Inspector

    [Space(10.0f)]
    [Tooltip("The player's point of view")]
    public GameObject Pov;

    [Tooltip("Input parameters")]
    public InputInfo InputAxis = new InputInfo()
    {
        Move = "Vertical",
        Strafe = "Horizontal"
    };

    [Tooltip("Walk parameters")]
    public GroundedMovementsInfo WalkParameters = new GroundedMovementsInfo()
    {
        Speed = 5F
    };

    #endregion

    private CharacterController _characterController;

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
    }
	
	void Update () 
    {
        Vector3 move, strafe;
        GetGroundAxis(out move, out strafe);

        move *= Input.GetAxis(InputAxis.Move);
        strafe *= Input.GetAxis(InputAxis.Strafe);
        
        var moveDirection = move + strafe;
        if (moveDirection.magnitude > 1F)
        {
            moveDirection.Normalize();
        }

        _characterController.Move(moveDirection * WalkParameters.Speed * Time.deltaTime);
	}

    [Serializable]
    public struct InputInfo
    {
        [Tooltip("Input name for the \"move\" axis")]
        public string Move;

        [Tooltip("Input name for the \"strafe\" axis")]
        public string Strafe;
    }

    [Serializable]
    public struct GroundedMovementsInfo
    {
        [Tooltip("The speed when grounded")]
        public float Speed;
    }
}
