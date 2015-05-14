using Common;
using System;
using UnityEngine;

public class InputController : MonoBehaviour, IInputController
{
    #region Inspector

    [Tooltip("The input axis and buttons names")]
    public AxisInfo AxisNames = new AxisInfo()
    {
        Forward = "Vertical",
        Strafe = "Horizontal",
        HoldWalk = "Walk",
        Jump = "Jump",
    };

    [Tooltip("The \"walk\" speed factor")]
    [Range(0F, 1F)]
    public float WalkFactor = 0.5F;
    
    #endregion

    /// <summary>
    /// Returns the forward axis value
    /// </summary>
    public float Forward
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns the strafe axis value
    /// </summary>
    public float Strafe
    {
        get;
        private set;
    }

    /// <summary>
    /// Returns a value indicating whether the player wants to jump
    /// </summary>
    public bool Jump
    {
        get;
        private set;
    }

    void Update()
    {
        var inputs = new Vector2(
            Input.GetAxis(AxisNames.Forward),
            Input.GetAxis(AxisNames.Strafe)
        );

        if (inputs.magnitude > 1F)
        {
            inputs.Normalize();
        }

        if (Input.GetButton(AxisNames.HoldWalk))
        {
            inputs *= WalkFactor;
        }

        Forward = inputs.x;
        Strafe = inputs.y;
        Jump = Input.GetButtonDown(AxisNames.Jump);
    }

    [Serializable]
    public struct AxisInfo
    {
        [Tooltip("The \"forward\" axis name")]
        public string Forward;

        [Tooltip("The \"strafe\" axis name")]
        public string Strafe;

        [Tooltip("The \"walk\" button name")]
        public string HoldWalk;

        [Tooltip("The \"jump\" button name")]
        public string Jump;
    }
}
