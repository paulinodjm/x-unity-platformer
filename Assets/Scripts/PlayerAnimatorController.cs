using UnityEngine;
using System.Collections;
using Common;
using System;

public class PlayerAnimatorController : MonoBehaviour, IAnimatorController
{
    #region Inspector

    public ParametersInfo ParameterNames = new ParametersInfo()
    {
        Gravity = "Gravity",
        InputMove = "InputMove",
        VelocityX = "VelocityX",
        VelocityY = "VelocityY",
        VelocityZ = "VelocityZ",
        RotationLerpFactor = "RotationLerpFactor",
    };

    #endregion

    #region IAnimatorController Members

    public float InputMove
    {
        set
        {
            _animator.SetFloat(ParameterNames.InputMove, value);
        }
    }

    public float Gravity
    {
        get
        {
            return _animator.GetFloat(ParameterNames.Gravity);
        }
    }

    public Vector3 Velocity
    {
        get
        {
            return new Vector3(
                _animator.GetFloat(ParameterNames.VelocityX),
                _animator.GetFloat(ParameterNames.VelocityY),
                _animator.GetFloat(ParameterNames.VelocityZ)
            );
        }
        set
        {
            _animator.SetFloat(ParameterNames.VelocityX, value.x);
            _animator.SetFloat(ParameterNames.VelocityY, value.y);
            _animator.SetFloat(ParameterNames.VelocityZ, value.z);
        }
    }

    public float RotationLerpFactor
    {
        get
        {
            return _animator.GetFloat(ParameterNames.RotationLerpFactor);
        }
    }

    #endregion

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    #region Trucs privés

    /// <summary>
    /// Contient les noms des différents paramètres de l'animator controller
    /// </summary>
    [Serializable]
    public struct ParametersInfo
    {
        public string InputMove;

        public string Gravity;

        public string VelocityX;

        public string VelocityY;

        public string VelocityZ;

        public string RotationLerpFactor;
    }

    #endregion
}
