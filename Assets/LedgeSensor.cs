using UnityEngine;
using System.Collections.Generic;

public class LedgeSensor : MonoBehaviour
{
    #region Inspector

    [Tooltip("The margin kept between a ledge and the player")]
    public float Margin = 0F;

    [Tooltip("[0; infinite]")]
    public float MaxClimbDownHeight = 1F;

    [Tooltip("The layer mask used to check the collisions")]
    public LayerMask CollisionLayers;

    #endregion

    /// <summary>
    /// Returns the ledges near the character.
    /// </summary>
    public List<Ledge> Ledges { get; private set; }

    /// <summary>
    /// Returns the positions the character can reach using the legdes.
    /// </summary>
    public List<Vector3> TargetPositions { get; private set; }

    private CharacterController _characterController;

    public LedgeSensor()
    {
        TargetPositions = new List<Vector3>();
    }

    void Awake()
    {
        Ledges = new List<Ledge>();
        _characterController = GetComponent<CharacterController>();
    }

    void OnTriggerEnter(Collider collider)
    {
        var ledge = collider.gameObject.GetComponentInParent<Ledge>();
        if (ledge == null) return;

        Ledges.Add(ledge);
    }

    void OnTriggerExit(Collider collider)
    {
        var ledge = collider.gameObject.GetComponentInParent<Ledge>();
        if (ledge == null) return;

        Ledges.Remove(ledge);
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = new Color(1F, 0F, 0F, 0.5F);

        foreach (var position in TargetPositions)
        {
            var drawPosition = position;
            drawPosition.y += _characterController.radius;
            Gizmos.DrawSphere(drawPosition, _characterController.radius);
        }
    }

    void Update()
    {
        TargetPositions.Clear();

        foreach (var ledge in Ledges)
        {
            var calculator = new LedgeGrabCalculator(_characterController, ledge, Margin);

            Vector3 targetPosition;
            if (!CalcTargetPosition(calculator, out targetPosition) || IsStep(targetPosition))
                continue;

            TargetPositions.Add(targetPosition);
        }
    }

    /// <summary>
    /// Calculates the position the character will reach if he uses the ledge
    /// </summary>
    /// <param name="ledgeInfo">The ledge grabbing informations</param>
    /// <param name="targetPosition">The target position</param>
    /// <returns>A value indicating whether the climb is possible; if false, the target position is not valid.</returns>
    private bool CalcTargetPosition(LedgeGrabCalculator ledgeInfo, out Vector3 targetPosition)
    {
        var footPosition = ledgeInfo.GrabPosition + (ledgeInfo.GrabDirection * (_characterController.radius + Margin));
        footPosition.y += Margin;

        var topPosition = footPosition;
        topPosition.y += _characterController.height - _characterController.radius;

        var bottomPosition = footPosition;
        bottomPosition.y += _characterController.radius + Margin;

        if (Physics.CheckCapsule(topPosition, bottomPosition, _characterController.radius, CollisionLayers))
        {
            targetPosition = Vector3.zero;
            return false;
        }

        RaycastHit hitInfo;
        if (Physics.CapsuleCast(topPosition, bottomPosition, _characterController.radius, Vector3.down, out hitInfo, MaxClimbDownHeight, CollisionLayers))
        {
            targetPosition = footPosition;
            targetPosition.y = hitInfo.point.y;
        }
        else
        {
            targetPosition = bottomPosition;
            targetPosition.y -= _characterController.radius + MaxClimbDownHeight;
        }
        return true;
    }

    /// <summary>
    /// Tell if the target position is a simple step or a real obstacle
    /// </summary>
    /// <param name="target">The target position</param>
    /// <returns>True if the position is a step; false otherwise</returns>
    private bool IsStep(Vector3 target)
    {
        return Mathf.Abs(target.y - transform.position.y + Margin) <= _characterController.stepOffset;
    }

    /// <summary>
    /// Performs the calculations to determine the grab position
    /// </summary>
    private class LedgeGrabCalculator
    {
        /// <summary>
        /// Return the ledge
        /// </summary>
        public Ledge Ledge
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the character
        /// </summary>
        public CharacterController Character
        {
            get;
            private set;
        }

        /// <summary>
        /// The margin kept between the ledge and the character
        /// </summary>
        public float Margin
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value that tell whether the data are valid or not.<br/>
        /// The object should not be used if not.
        /// </summary>
        public bool IsValid
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the perpendicular direction to reach the ledge
        /// </summary>
        public Vector3 GrabDirection
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the grab position
        /// </summary>
        public Vector3 GrabPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new calculator
        /// </summary>
        /// <param name="character">The character</param>
        /// <param name="ledge">The ledge</param>
        /// <param name="margin">The margin</param>
        public LedgeGrabCalculator(CharacterController character, Ledge ledge, float margin = 0F)
        {
            Ledge = ledge;
            Character = character;
            Margin = margin;

            _radius = Character.radius + margin;

            CalcRelativePosition();
            CalcRawGrabPosition();
            CalcGrabDirection();
            ValidateRawGrabPosition();
            CalcSafeGrabPosition();
            CalcFinalGrabPosition();
        }

        /// <summary>
        /// The used character radius
        /// </summary>
        private float _radius;

        /// <summary>
        /// The character position, relative to the ledge and in a flat system
        /// </summary>
        private Vector3 _relativePosition;

        /// <summary>
        /// The raw grab position, relative to the ledge and in a flat system
        /// </summary>
        private Vector3 _rawGrabPosition;

        /// <summary>
        /// The safe grab position in a flat system
        /// </summary>
        private Vector3 _safeGrabPosition;

        /// <summary>
        /// Calculates the character position, relative to the ledge and in a flat system
        /// </summary>
        private void CalcRelativePosition()
        {
            var position = Character.transform.position - Ledge.Start;
            position.y = 0;
            _relativePosition = position;
        }

        /// <summary>
        /// Calculates the raw grab position
        /// </summary>
        private void CalcRawGrabPosition()
        {
            _rawGrabPosition = Vector3.Project(-_relativePosition, Ledge.transform.forward) + _relativePosition;
        }

        /// <summary>
        /// Calculates the grab direction
        /// </summary>
        private void CalcGrabDirection()
        {
            GrabDirection = (_rawGrabPosition - _relativePosition).normalized;
        }

        /// <summary>
        /// Tell if the computation can continue, or if the grab position is outside the ledge
        /// </summary>
        /// <returns>A value that indicate whather the data are valid</returns>
        private bool ValidateRawGrabPosition()
        {
            if (_rawGrabPosition.normalized == -Ledge.FlatDirection)
            {
                _rawGrabPosition = Vector3.zero;
                IsValid = false;
            }
            else if (_rawGrabPosition.magnitude > Ledge.FlatLength)
            {
                _rawGrabPosition = Ledge.FlatEnd - Ledge.Start;
                IsValid = false;
            }
            else
            {
                IsValid = true;
            }

            return IsValid;
        }

        /// <summary>
        /// Calculates the safe grab position
        /// </summary>
        private void CalcSafeGrabPosition()
        {
            if (Ledge.FlatLength <= (_radius * 2))
            {
                _safeGrabPosition = Ledge.Start + (Ledge.FlatDirection * (Ledge.FlatLength / 2F));
            }
            else if (_rawGrabPosition.magnitude < _radius)
            {
                _safeGrabPosition = Ledge.Start + (Ledge.FlatDirection * _radius);
            }
            else if (_rawGrabPosition.magnitude > Ledge.FlatLength - _radius)
            {
                _safeGrabPosition = Ledge.FlatEnd - (Ledge.FlatDirection * _radius);
            }
            else
            {
                _safeGrabPosition = Ledge.Start + _rawGrabPosition;
            }
        }

        /// <summary>
        /// Calculates the final grab position
        /// </summary>
        private void CalcFinalGrabPosition()
        {
            var normalizedGrabPosition = Mathf.InverseLerp(0, Ledge.FlatLength, Vector3.Distance(Ledge.Start, _safeGrabPosition));
            var height = Mathf.Lerp(Ledge.Start.y, Ledge.End.y, normalizedGrabPosition);

            var grabPosition = _safeGrabPosition;
            grabPosition.y = height;
            GrabPosition = grabPosition;
        }
    }
}
