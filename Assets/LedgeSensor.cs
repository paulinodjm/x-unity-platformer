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

    [Tooltip("The minimum angle required to pass through a ledge")]
    [Range(0F, 90F)]
    public float LedgeBreakAngle = 45F;

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

    void Update()
    {
        GrabInfos.Clear();

        var character = CreateCharacterInfo(transform.position);

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
            if (!grabInfo.HasTargetPosition)
                continue;

            var deltaHeight = 0F;
            if (grabInfo.HasFromPosition)
            {
                deltaHeight = grabInfo.TargetPosition.y - grabInfo.FromPosition.y;
            }

            if (grabInfo.TrueGrabDistance < _characterController.radius * FallThresehold
             || (grabInfo.IsValid && grabInfo.TrueGrabDistance < _characterController.radius && deltaHeight > 0F))
            {
                if (nearestLedge == null || grabInfo.TrueGrabDistance < nearestLedge.TrueGrabDistance)
                {
                    nearestLedge = grabInfo;
                }
            }
        }

        return nearestLedge;
    }

    public void ConstraintMove(ref Vector3 move, Vector3 desiredDirection)
    {
        var ledgeBreakValue = Mathf.Sin(LedgeBreakAngle * Mathf.Deg2Rad);

        var character = CreateCharacterInfo(Vector3.zero);
        var radius = _characterController.radius * FallThresehold + Margin;

        foreach (var grabInfo in GrabInfos)
        {
            var inputProjection = Vector3.Project(desiredDirection, grabInfo.PerpendicularGrabDirection);
            if (inputProjection.magnitude >= ledgeBreakValue)
                continue;

            var nextPosition = transform.position + move;
            character.Position = nextPosition;

            var nextledgeGrab = new LedgeGrabCalculator(grabInfo.Ledge, character, Margin);

            var overlap = nextledgeGrab.TrueGrabDistance - radius;
            if (overlap < 0F)
            {
                move += nextledgeGrab.TrueGrabDirection.normalized * overlap;
                character.Position = transform.position + move;
            }
        }
    }

    /// <summary>
    /// Creates a character info instance for the given position
    /// </summary>
    /// <param name="position">The character position</param>
    /// <returns>The character info</returns>
    private CharacterInfo CreateCharacterInfo(Vector3 position)
    {
        return new CharacterInfo()
        {
            Height = _characterController.height,
            Position = position,
            Radius = _characterController.radius,
            FallRadiusFactor = FallThresehold,
            StepOffset = _characterController.stepOffset,
        };
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

        Vector3 PerpendicularGrabDirection
        {
            get;
        }

        float PerpendicularGrabDistance
        {
            get;
        }

        Vector3 TrueGrabDirection
        {
            get;
        }

        float TrueGrabDistance
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

        public Vector3 PerpendicularGrabDirection
        {
            get
            {
                return LedgeGrabCalculator.PerpendicularGrabDirection;
            }
        }

        public float TrueGrabDistance
        {
            get
            {
                return LedgeGrabCalculator.TrueGrabDistance;
            }
        }

        public Vector3 GrabPosition
        {
            get
            {
                return LedgeGrabCalculator.GrabPosition;
            }
        }

        public float PerpendicularGrabDistance
        {
            get
            {
                return LedgeGrabCalculator.PerpendicularGrabDistance;
            }
        }

        public Vector3 TrueGrabDirection
        {
            get
            {
                return LedgeGrabCalculator.TrueGrabDirection;
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
        public Vector3 PerpendicularGrabDirection
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the distance between the character and the grab position
        /// </summary>
        public float TrueGrabDistance
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

        public float PerpendicularGrabDistance
        {
            get;
            private set;
        }

        public Vector3 TrueGrabDirection
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
            var grabVector = _rawGrabPosition - _relativePosition;

            PerpendicularGrabDirection = grabVector.normalized;
            PerpendicularGrabDistance = grabVector.magnitude;
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
                TrueGrabDistance = Vector3.Distance(_relativePosition, _rawGrabPosition);
                TrueGrabDirection = (_rawGrabPosition - _relativePosition).normalized;
            }
            else
            {
                var ledgeEndDistance = Vector3.Distance(Ledge.FlatEnd - Ledge.Start, _relativePosition);

                if (_relativePosition.magnitude < ledgeEndDistance)
                {
                    TrueGrabDistance = _relativePosition.magnitude;
                    TrueGrabDirection = -_relativePosition.normalized;
                }
                else
                {
                    TrueGrabDistance = ledgeEndDistance;
                    TrueGrabDirection = (Ledge.FlatEnd - Ledge.Start - _relativePosition).normalized;
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

            CalcFirstChangeTargetPosition();
            CalcFirstChangeFromPosition();
            CalcPositions();
            CalcIsStep();
        }

        /// <summary>
        /// Perform the target position check
        /// </summary>
        private void CalcFirstChangeTargetPosition()
        {
            Vector3 targetPosition;
            var offset = GrabInfo.PerpendicularGrabDirection * (GrabInfo.Character.Radius + GrabInfo.Margin);

            HasTargetPosition = PerformCapsuleCheck(out targetPosition, offset);
            TargetPosition = targetPosition;
        }

        /// <summary>
        /// Perform the start position collision check
        /// </summary>
        private void CalcFirstChangeFromPosition()
        {
            Vector3 fromPosition;
            var offset = -GrabInfo.PerpendicularGrabDirection * (GrabInfo.Character.Radius + GrabInfo.Margin);

            HasFromPosition = PerformCapsuleCheck(out fromPosition, offset);
            FromPosition = fromPosition;
        }

        private void CalcPositions()
        {
            Vector3 position;
            var offset = GrabInfo.PerpendicularGrabDirection * (GrabInfo.Character.FallRadius + GrabInfo.Margin);

            if (HasFromPosition && HasTargetPosition)
            {
                if (TargetPosition.y > FromPosition.y)
                {
                    HasTargetPosition = PerformCapsuleCheck(out position, offset);
                    TargetPosition = position;
                }
                else
                {
                    HasFromPosition = PerformCapsuleCheck(out position, -offset);
                    FromPosition = position;
                }
            }
            else if (!HasFromPosition && !HasTargetPosition)
            {
                HasFromPosition = PerformCapsuleCheck(out position, -offset);
                FromPosition = position;

                if (!HasTargetPosition)
                {
                    HasTargetPosition = PerformCapsuleCheck(out position, offset);
                    TargetPosition = position;
                }
                return;
            }
            else
            {
                if (PerformCapsuleCheck(out position, offset))
                {
                    if (!HasTargetPosition || position.y <= TargetPosition.y)
                    {
                        HasTargetPosition = true;
                        TargetPosition = position;
                    }
                }
                else if (PerformCapsuleCheck(out position, -offset))
                {
                    if (!HasFromPosition || position.y <= FromPosition.y)
                    {
                        HasFromPosition = true;
                        FromPosition = position;
                    }
                }
                else
                {
                    // nothing to do, both are checked
                }
            }
        }

        private bool PerformCapsuleCheck(out Vector3 position, Vector3 offset)
        {
            var footPosition = GrabInfo.GrabPosition + offset;
            footPosition.y += GrabInfo.Margin;

            var topPosition = footPosition;
            topPosition.y += GrabInfo.Character.Height - GrabInfo.Character.Radius;

            var bottomPosition = footPosition;
            bottomPosition.y += GrabInfo.Character.Radius + GrabInfo.Margin;

            if (Physics.CheckCapsule(topPosition, bottomPosition, GrabInfo.Character.Radius, CollisionLayers))
            {
                position = Vector3.zero;
                return false;
            }

            RaycastHit hitInfo;
            if (Physics.CapsuleCast(topPosition, bottomPosition, GrabInfo.Character.Radius, Vector3.down, out hitInfo, MaxClimbDownHeight, CollisionLayers))
            {
                position = footPosition;
                position.y = hitInfo.point.y;
            }
            else
            {
                position = bottomPosition;
                position.y -= GrabInfo.Character.Radius + MaxClimbDownHeight;
            }

            return true;
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
        /// Get or set the fall radius factor
        /// </summary>
        public float FallRadiusFactor
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

        /// <summary>
        /// Returns the fall radius
        /// </summary>
        public float FallRadius
        {
            get
            {
                return Radius * FallRadiusFactor;
            }
        }
    }

    #endregion
}
