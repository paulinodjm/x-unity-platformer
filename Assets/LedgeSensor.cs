using UnityEngine;
using System.Collections.Generic;

public class LedgeSensor : MonoBehaviour
{
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
            var calculator = new LedgeGrabCalculator(_characterController, ledge);

            if (!calculator.IsValid)
                continue;

            TargetPositions.Add(calculator.GrabPosition + (calculator.GrabDirection * _characterController.radius));
        }
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
        public LedgeGrabCalculator(CharacterController character, Ledge ledge)
        {
            Ledge = ledge;
            Character = character;

            CalcRelativePosition();
            CalcRawGrabPosition();
            CalcGrabDirection();
            if (!ValidateRawGrabPosition())
                return;
            CalcSafeGrabPosition();
            CalcFinalGrabPosition();
        }

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
                IsValid = false;
            }
            else
            {
                IsValid = _rawGrabPosition.magnitude <= Ledge.FlatLength;
            }
            return IsValid;
        }

        /// <summary>
        /// Calculates the safe grab position
        /// </summary>
        private void CalcSafeGrabPosition()
        {
            if (Ledge.FlatLength <= (Character.radius * 2))
            {
                _safeGrabPosition = Ledge.Start + (Ledge.FlatDirection * (Ledge.FlatLength / 2F));
            }
            else if (_rawGrabPosition.magnitude < Character.radius)
            {
                _safeGrabPosition = Ledge.Start + (Ledge.FlatDirection * Character.radius);
            }
            else if (_rawGrabPosition.magnitude > Ledge.FlatLength - Character.radius)
            {
                _safeGrabPosition = Ledge.FlatEnd - (Ledge.FlatDirection * Character.radius);
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
