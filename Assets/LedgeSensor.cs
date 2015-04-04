using UnityEngine;
using System.Collections.Generic;
using System;

public class LedgeSensor : MonoBehaviour
{
    #region Inspector

    [Tooltip("The margin kept between a ledge and the player")]
    public float Margin = 0F;

    [Tooltip("[0; infinite]")]
    public float MaxClimbDownHeight = 1F;

    [Tooltip("The radius factor used as thresehold to detemine when the character must fall from a ledge.")]
    [Range(0F, 1F)]
    public float FallThresehold = 1F;

    [Tooltip("The layer mask used to check the collisions")]
    public LayerMask CollisionLayers;

    #endregion

    /// <summary>
    /// Returns the ledges near the character.
    /// </summary>
    public List<Ledge> Ledges { get; private set; }

    /// <summary>
    /// Returns all the grab informations
    /// </summary>
    public List<IGrabInfo> GrabInfos { get; private set; }

    private CharacterController _characterController;

    public LedgeSensor()
    {
        GrabInfos = new List<IGrabInfo>();
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

        foreach (var grabInfo in GrabInfos)
        {
            if (grabInfo.HasTargetPosition)
            {
                var drawPosition = grabInfo.TargetPosition;
                drawPosition.y += _characterController.radius;

                Gizmos.color = grabInfo.IsValid ? new Color(0F, 1F, 0F, 0.5F) : new Color(1F, 0F, 0F, 0.5F);
                Gizmos.DrawSphere(drawPosition, _characterController.radius);
            }

            if (grabInfo.HasFromPosition)
            {
                var drawPosition = grabInfo.FromPosition;
                drawPosition.y += _characterController.radius;

                Gizmos.color = new Color(0F, 0.5F, 1F, 0.5F);
                Gizmos.DrawSphere(drawPosition, _characterController.radius);
            }

            if (grabInfo.GrabDistance <= _characterController.radius * FallThresehold)
            {
                Debug.DrawLine(grabInfo.Ledge.Start, grabInfo.Ledge.End, Color.red);
            }
        }
    }

    void Update()
    {
        GrabInfos.Clear();

        var character = new CharacterInfo()
        {
            Height = _characterController.height,
            Position = transform.position,
            Radius = _characterController.radius,
            StepOffset = _characterController.stepOffset,
        };

        foreach (var ledge in Ledges)
        {
            var calculator = new LedgeGrabCalculator(ledge, character, Margin);
            var checker = new ClimbPositionChecker(calculator, CollisionLayers, MaxClimbDownHeight);

            if (checker.IsStep)
                continue;

            var info = new GrabInfo(ledge, calculator, checker);
            GrabInfos.Add(info);
        }
    }

    /// <summary>
    /// Returns the ledge from witch the character must fall, if any
    /// </summary>
    /// <returns>The ledge; null if none</returns>
    public IGrabInfo GetFallingLedge()
    {
        IGrabInfo nearestLedge = null;

        foreach (var grabInfo in GrabInfos)
        {
            if (grabInfo.GrabDistance <= _characterController.radius * FallThresehold)
            {
                if (nearestLedge == null || grabInfo.GrabDistance < nearestLedge.GrabDistance)
                {
                    nearestLedge = grabInfo;
                }
            }
        }

        return nearestLedge;
    }

    #region Public interfaces

    public interface IGrabInfo : ILedgeGrabCalculator, IClimbPositionChecker
    {
        Ledge Ledge
        {
            get;
        }
    }

    public interface ILedgeGrabCalculator
    {
        bool IsValid
        {
            get;
        }

        Vector3 GrabDirection
        {
            get;
        }

        float GrabDistance
        {
            get;
        }

        Vector3 GrabPosition
        {
            get;
        }
    }

    public interface IClimbPositionChecker
    {
        bool IsStep
        {
            get;
        }

        bool HasTargetPosition
        {
            get;
        }

        Vector3 TargetPosition
        {
            get;
        }

        bool HasFromPosition
        {
            get;
        }

        Vector3 FromPosition
        {
            get;
        }
    }

    #endregion

    #region Private classes

    private class GrabInfo : IGrabInfo
    {
        public ILedgeGrabCalculator LedgeGrabCalculator
        {
            get;
            private set;
        }

        public IClimbPositionChecker ClimbPositionChecker
        {
            get;
            private set;
        }

        public GrabInfo(Ledge ledge, ILedgeGrabCalculator ledgeGrabCalculator, IClimbPositionChecker climbPositionChecker)
        {
            Ledge = ledge;
            LedgeGrabCalculator = ledgeGrabCalculator;
            ClimbPositionChecker = climbPositionChecker;
        }

        #region IGrabInfo Membres

        public Ledge Ledge
        {
            get;
            private set;
        }

        #endregion

        #region ILedgeGrabCalculator Membres

        public bool IsValid
        {
            get 
            {
                return LedgeGrabCalculator.IsValid;
            }
        }

        public Vector3 GrabDirection
        {
            get 
            {
                return LedgeGrabCalculator.GrabDirection;
            }
        }

        public float GrabDistance
        {
            get 
            {
                return LedgeGrabCalculator.GrabDistance;
            }
        }

        public Vector3 GrabPosition
        {
            get 
            {
                return LedgeGrabCalculator.GrabPosition;
            }
        }

        #endregion

        #region IClimbPositionChecker Membres

        public bool IsStep
        {
            get 
            {
                return ClimbPositionChecker.IsStep;
            }
        }

        public bool HasTargetPosition
        {
            get 
            {
                return ClimbPositionChecker.HasTargetPosition;
            }
        }

        public Vector3 TargetPosition
        {
            get 
            {
                return ClimbPositionChecker.TargetPosition;
            }
        }

        public bool HasFromPosition
        {
            get 
            {
                return ClimbPositionChecker.HasFromPosition;
            }
        }

        public Vector3 FromPosition
        {
            get 
            {
                return ClimbPositionChecker.FromPosition;
            }
        }

        #endregion
    }

    /// <summary>
    /// Performs the calculations to determine the grab position on a ledge
    /// </summary>
    private class LedgeGrabCalculator : ILedgeGrabCalculator
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
        /// Returns the character informations
        /// </summary>
        public CharacterInfo Character
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
        /// Returns the distance between the character and the grab position
        /// </summary>
        public float GrabDistance
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
        /// <param name="ledge">The ledge</param>
        /// <param name="character">The character informations</param>
        /// <param name="margin">The margin</param>
        public LedgeGrabCalculator(Ledge ledge, CharacterInfo character, float margin = 0F)
        {
            Ledge = ledge;
            Character = character;
            Margin = margin;

            _radius = Character.Radius + margin;

            CalcRelativePosition();
            CalcRawGrabPosition();
            CalcGrabDirection();
            ValidateRawGrabPosition();
            CalcSafeGrabPosition();
            CalcFinalGrabPosition();
            CalcCrossing();
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
            var position = Character.Position - Ledge.Start;
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

        /// <summary>
        /// Initialize the GrabDistance property (and makes the object able to check if the character crosses the ledge)
        /// </summary>
        private void CalcCrossing()
        {
            if (IsValid)
            {
                GrabDistance = Vector3.Distance(_relativePosition, _rawGrabPosition);
            }
            else
            {
                var ledgeEndDistance = Vector3.Distance(Ledge.FlatEnd - Ledge.Start, _relativePosition);

                if (_relativePosition.magnitude < ledgeEndDistance)
                {
                    GrabDistance = _relativePosition.magnitude;
                }
                else
                {
                    GrabDistance = ledgeEndDistance;
                }
            }
        }
    }

    /// <summary>
    /// Performs the collision checks
    /// </summary>
    private class ClimbPositionChecker : IClimbPositionChecker
    {
        /// <summary>
        /// Returns the grab informations
        /// </summary>
        public LedgeGrabCalculator GrabInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the mask used to check the collisions
        /// </summary>
        public LayerMask CollisionLayers
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the maximum height the character can climb down
        /// </summary>
        public float MaxClimbDownHeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value indicatig whether using the ledge is like climbing a simple step
        /// </summary>
        public bool IsStep
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the target position
        /// </summary>
        public Vector3 TargetPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value indicating whether the target position is valid or not
        /// </summary>
        public bool HasTargetPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the start position
        /// </summary>
        public Vector3 FromPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value indicating whether the start position is valid or not
        /// </summary>
        public bool HasFromPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new collision checker
        /// </summary>
        /// <param name="grabInfo">The grab informations</param>
        /// <param name="collisionLayers">The mask used to check the collisions</param>
        /// <param name="maxClimbDownHeight">The maximum height the character can climb down</param>
        public ClimbPositionChecker(LedgeGrabCalculator grabInfo, LayerMask collisionLayers, float maxClimbDownHeight)
        {
            GrabInfo = grabInfo;
            CollisionLayers = collisionLayers;
            MaxClimbDownHeight = maxClimbDownHeight;

            CalcTargetPosition();
            CalcFromPosition();
            CalcIsStep();
        }

        /// <summary>
        /// Perform the target position check
        /// </summary>
        private void CalcTargetPosition()
        {
            var footPosition = GrabInfo.GrabPosition + (GrabInfo.GrabDirection * (GrabInfo.Character.Radius + GrabInfo.Margin));
            footPosition.y += GrabInfo.Margin;

            var topPosition = footPosition;
            topPosition.y += GrabInfo.Character.Height - GrabInfo.Character.Radius;

            var bottomPosition = footPosition;
            bottomPosition.y += GrabInfo.Character.Radius + GrabInfo.Margin;

            if (Physics.CheckCapsule(topPosition, bottomPosition, GrabInfo.Character.Radius, CollisionLayers))
            {
                TargetPosition = Vector3.zero;
                HasTargetPosition = false;
                return;
            }

            Vector3 targetPosition;
            RaycastHit hitInfo;
            if (Physics.CapsuleCast(topPosition, bottomPosition, GrabInfo.Character.Radius, Vector3.down, out hitInfo, MaxClimbDownHeight, CollisionLayers))
            {
                targetPosition = footPosition;
                targetPosition.y = hitInfo.point.y;
            }
            else
            {
                targetPosition = bottomPosition;
                targetPosition.y -= GrabInfo.Character.Radius + MaxClimbDownHeight;
            }

            TargetPosition = targetPosition;
            HasTargetPosition = true;
        }

        /// <summary>
        /// Perform the start position collision check
        /// </summary>
        private void CalcFromPosition()
        {
            var footPosition = GrabInfo.GrabPosition - (GrabInfo.GrabDirection * (GrabInfo.Character.Radius + GrabInfo.Margin));
            footPosition.y += GrabInfo.Margin;

            var topPosition = footPosition;
            topPosition.y += GrabInfo.Character.Height - GrabInfo.Character.Radius;

            var bottomPosition = footPosition;
            bottomPosition.y += GrabInfo.Character.Radius + GrabInfo.Margin;

            if (Physics.CheckCapsule(topPosition, bottomPosition, GrabInfo.Character.Radius, CollisionLayers))
            {
                FromPosition = Vector3.zero;
                HasFromPosition = false;
                return;
            }

            Vector3 fromPosition;
            RaycastHit hitInfo;
            if (Physics.CapsuleCast(topPosition, bottomPosition, GrabInfo.Character.Radius, Vector3.down, out hitInfo, MaxClimbDownHeight, CollisionLayers))
            {
                fromPosition = footPosition;
                fromPosition.y = hitInfo.point.y;
            }
            else
            {
                fromPosition = bottomPosition;
                fromPosition.y -= GrabInfo.Character.Radius + MaxClimbDownHeight;
            }

            FromPosition = fromPosition;
            HasFromPosition = true;
        }

        /// <summary>
        /// Initialize the IsStep property
        /// </summary>
        private void CalcIsStep()
        {
            if (HasFromPosition && HasTargetPosition)
            {
                IsStep = Mathf.Abs(TargetPosition.y - FromPosition.y) <= GrabInfo.Character.StepOffset;
            }
            else
            {
                IsStep = false;
            }
        }
    }

    /// <summary>
    /// Hold the character informations
    /// </summary>
    private class CharacterInfo
    {
        /// <summary>
        /// Get or set the character position
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the character radius
        /// </summary>
        public float Radius
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the character height
        /// </summary>
        public float Height
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the step offset
        /// </summary>
        public float StepOffset
        {
            get;
            set;
        }
    }

    #endregion
}
