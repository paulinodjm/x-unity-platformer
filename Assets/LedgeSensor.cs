using UnityEngine;
using System.Collections.Generic;

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
    public List<GrabInfo> GrabInfos { get; private set; }

    private CharacterController _characterController;

    public LedgeSensor()
    {
        GrabInfos = new List<GrabInfo>();
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
            var drawPosition = grabInfo.TargetPosition;
            drawPosition.y += _characterController.radius;

            Gizmos.color = grabInfo.IsInFront ? new Color(0F, 1F, 0F, 0.5F) : new Color(1F, 0F, 0F, 0.5F);
            Gizmos.DrawSphere(drawPosition, _characterController.radius);

            drawPosition = grabInfo.FromPosition;
            drawPosition.y += _characterController.radius;

            Gizmos.color = new Color(0F, 0.5F, 1F, 0.5F);
            Gizmos.DrawSphere(drawPosition, _characterController.radius);

            if (grabInfo.GrabDistance <= _characterController.radius * FallThresehold)
            {
                Debug.DrawLine(grabInfo.Ledge.Start, grabInfo.Ledge.End, Color.red);
            }
        }
    }

    void Update()
    {
        GrabInfos.Clear();

        foreach (var ledge in Ledges)
        {
            var calculator = new LedgeGrabCalculator(_characterController, ledge, Margin);
            var checker = new ClimbPositionChecker(calculator, CollisionLayers, MaxClimbDownHeight);

            if (!checker.HasTargetPosition || !checker.HasFromPosition || checker.IsStep)
                continue;

            var info = new GrabInfo(ledge, calculator.GrabPosition, calculator.GrabDirection, checker.FromPosition, checker.TargetPosition, calculator.IsValid, calculator.GrabDistance);
            GrabInfos.Add(info);
        }
    }

    /// <summary>
    /// Holds the grab informations
    /// </summary>
    public struct GrabInfo
    {
        /// <summary>
        /// Returns the ledge
        /// </summary>
        public Ledge Ledge
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
        /// Returns the grab direction
        /// </summary>
        public Vector3 GrabDirection
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
        /// Returns the start position
        /// </summary>
        public Vector3 FromPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value indicating whether the player is in front of the ledge
        /// </summary>
        public bool IsInFront
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value indicatig whether the ledge is a simple step
        /// </summary>
        public bool IsStep
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value telling whether the character is crossing the edge or not
        /// </summary>
        public float GrabDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new grab info
        /// </summary>
        /// <param name="ledge">The ledge</param>
        /// <param name="grabPosition">The grab position</param>
        /// <param name="grabDirection">The grab direction</param>
        /// <param name="fromPosition">The start position</param>
        /// <param name="targetPosition">The target position</param>
        /// <param name="isInFront">A value indicating whether the player is in front of the ledge</param>
        /// <param name="grabDistance">The distance to the grab position</param>
        public GrabInfo(Ledge ledge, Vector3 grabPosition, Vector3 grabDirection, Vector3 fromPosition, Vector3 targetPosition, bool isInFront, float grabDistance)
        {
            Ledge = ledge;
            GrabPosition = grabPosition;
            GrabDirection = grabDirection;
            FromPosition = fromPosition;
            TargetPosition = targetPosition;
            IsInFront = isInFront;
            GrabDistance = grabDistance;
        }
    }

    /// <summary>
    /// Performs the calculations to determine the grab position on a ledge
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
    private class ClimbPositionChecker
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
            var footPosition = GrabInfo.GrabPosition + (GrabInfo.GrabDirection * (GrabInfo.Character.radius + GrabInfo.Margin));
            footPosition.y += GrabInfo.Margin;

            var topPosition = footPosition;
            topPosition.y += GrabInfo.Character.height - GrabInfo.Character.radius;

            var bottomPosition = footPosition;
            bottomPosition.y += GrabInfo.Character.radius + GrabInfo.Margin;

            if (Physics.CheckCapsule(topPosition, bottomPosition, GrabInfo.Character.radius, CollisionLayers))
            {
                TargetPosition = Vector3.zero;
                HasTargetPosition = false;
                return;
            }

            Vector3 targetPosition;
            RaycastHit hitInfo;
            if (Physics.CapsuleCast(topPosition, bottomPosition, GrabInfo.Character.radius, Vector3.down, out hitInfo, MaxClimbDownHeight, CollisionLayers))
            {
                targetPosition = footPosition;
                targetPosition.y = hitInfo.point.y;
            }
            else
            {
                targetPosition = bottomPosition;
                targetPosition.y -= GrabInfo.Character.radius + MaxClimbDownHeight;
            }

            TargetPosition = targetPosition;
            HasTargetPosition = true;
        }

        /// <summary>
        /// Perform the start position collision check
        /// </summary>
        private void CalcFromPosition()
        {
            var footPosition = GrabInfo.GrabPosition - (GrabInfo.GrabDirection * (GrabInfo.Character.radius + GrabInfo.Margin));
            footPosition.y += GrabInfo.Margin;

            var topPosition = footPosition;
            topPosition.y += GrabInfo.Character.height - GrabInfo.Character.radius;

            var bottomPosition = footPosition;
            bottomPosition.y += GrabInfo.Character.radius + GrabInfo.Margin;

            if (Physics.CheckCapsule(topPosition, bottomPosition, GrabInfo.Character.radius, CollisionLayers))
            {
                FromPosition = Vector3.zero;
                HasFromPosition = false;
                return;
            }

            Vector3 fromPosition;
            RaycastHit hitInfo;
            if (Physics.CapsuleCast(topPosition, bottomPosition, GrabInfo.Character.radius, Vector3.down, out hitInfo, MaxClimbDownHeight, CollisionLayers))
            {
                fromPosition = footPosition;
                fromPosition.y = hitInfo.point.y;
            }
            else
            {
                fromPosition = bottomPosition;
                fromPosition.y -= GrabInfo.Character.radius + MaxClimbDownHeight;
            }

            FromPosition = fromPosition;
            HasFromPosition = true;
        }

        /// <summary>
        /// Initialize the IsStep property
        /// </summary>
        private void CalcIsStep()
        {
            IsStep = Mathf.Abs(TargetPosition.y - FromPosition.y) <= GrabInfo.Character.stepOffset;
        }
    }
}
