using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BounceTest : MonoBehaviour
{
    [Header("Velocity Sensor")]
    [Tooltip("Whether the move sensor is used")]
    public bool UseVelocity = true;
    [Tooltip("The move sensor; if not provided, the object itselfs")]
    public Transform Sensor;

    [Header("Gravity")]
    [Tooltip("Whether the gravity is used")]
    public bool UseGravity = true;
    [Tooltip("The used gravity force")]
    public Vector3 Gravity;

    [Header("Debug")]
    public float RotationSpeed;

    [Tooltip("The acceleration equivalent to one time the gravity (0 = no scaling)")]
    public float Scale;

    public float Accel;
    public float MaxSpeed;
    public float Friction;
    private float _speed;

    private List<string> _messages ;

    public float SpeedScale;

    /// <summary>
    /// Retourne la transformation actuellement utilisée comme capteur (n'est jamais nulle)
    /// </summary>
    public Transform CurrentSensor
    {
        get
        {
            return (Sensor != null ? Sensor : transform);
        }
    }

    /// <summary>
    /// Retourne la vélocité du capteur (coordonnées locales)
    /// </summary>
    public Vector3 SensorVelocity
    {
        get
        {
            return _sensorVelocity;
        }
    }
    private Vector3 _sensorVelocity;

    /// <summary>
    /// Retourne l'accélération ressentie (coordonnées locales)
    /// </summary>
    public Vector3 AccelerationFelt
    {
        get
        {
            return _accelerationFelt;
        }
    }
    private Vector3 _accelerationFelt;

    void Awake()
    {
        _messages = new List<string>();
    }

    void Start()
    {
        _previousSensorPosition = CurrentSensor.position;
    }

    void Update()
    {
        _messages.Clear();

        var move = Input.GetAxis("Vertical") * MaxSpeed;
        var strafe = Input.GetAxis("Horizontal") * MaxSpeed;

        transform.Translate(Vector3.forward * move * Time.deltaTime);
        transform.Translate(Vector3.right * strafe * Time.deltaTime);

        DetectMove();
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(CurrentSensor.position, CurrentSensor.rotation, CurrentSensor.lossyScale);

        // affiche les axes
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.down * 2, Vector3.up * 2);
        Gizmos.DrawCube(Vector3.up, Vector3.one / 10f);
        Gizmos.DrawCube(Vector3.down, Vector3.one / 10f);

        Gizmos.DrawLine(Vector3.left, Vector3.right);
        Gizmos.DrawLine(Vector3.back, Vector3.forward);

        // affichage de l'accélération
        var accelerationFactor = (Scale != 0 ? AccelerationFelt / Scale : AccelerationFelt);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(accelerationFactor, .1f);
    }

    void OnGUI()
    {
        GUILayout.Label(_messages.Count + " messages:");
        foreach (var message in _messages)
        {
            GUILayout.Label(message);
        }
    }

    #region Détection des mouvements du capteur

    /// <summary>
    /// Position précédente du sensor; utilisée pour en connaître la vélocité
    /// </summary>
    private Vector3 _previousSensorPosition;

    /// <summary>
    /// Récupère la vélocité du capteur dans l'espace de coordonnées global
    /// </summary>
    /// <returns>La vélocité du capteur (en unités par secondes)</returns>
    private Vector3 PullGlobalSensorVelocity()
    {
        if (!UseVelocity)
            return Vector3.zero;

        var globalSensorMove = (CurrentSensor.position - _previousSensorPosition);
        _previousSensorPosition = CurrentSensor.position;

        return globalSensorMove / Time.deltaTime; // vitesse -> (u/s)
    }

    /// <summary>
    /// Récupère la valeur correspondant à la gravité dans l'espace de coordonnées global
    /// </summary>
    /// <returns>La gravité (en unités par secondes²)</returns>
    private Vector3 PullGlobalGravity()
    {
        return (UseGravity ? Gravity : Vector3.zero); // accélération -> (u/s²)
    }

    /// <summary>
    /// Détecte les mouvements effectués par le capteur depuis la dernière update,
    /// et met à jour la valeur du <see cref="SensorVelocity"/>
    /// </summary>
    private void DetectMove()
    {
        var globalSensorVelocity = PullGlobalSensorVelocity();

        // calcule la vélocité
        var previousSensorVelocity = _sensorVelocity;
        var currentSensorVelocity = ProjectMove(globalSensorVelocity, CurrentSensor);
        _sensorVelocity = currentSensorVelocity;

        var deltaVelocity = _sensorVelocity - previousSensorVelocity;
        var acceleration = deltaVelocity / Time.deltaTime;

        // la gravité
        var globalGravity = PullGlobalGravity();
        var gravity = ProjectMove(globalGravity, CurrentSensor);

        // accélération résultante
        _accelerationFelt = gravity - acceleration; // une accélération égale à la gravité l'"annule", en quelque sorte

        // debug
        _messages.Add(
            String.Format("X) {0:-000.000000}u/s -> {1:000.000000}u/s | {2:000.000000}u : {3:000.000000}u/s² | {4:000.000000}u/s² | {5:000.000000}u/s²",
            previousSensorVelocity.x,
            currentSensorVelocity.x,
            deltaVelocity.x,
            acceleration.x,
            gravity.x,
            _accelerationFelt.x
        ));
        _messages.Add(
            String.Format("Y) {0:000.000000}u/s -> {1:000.000000}u/s | {2:000.000000}u : {3:000.000000}u/s² | {4:000.000000}u/s² | {5:000.000000}u/s²",
            previousSensorVelocity.y,
            currentSensorVelocity.y,
            deltaVelocity.y,
            acceleration.y,
            gravity.y,
            _accelerationFelt.y
        ));
        _messages.Add(
            String.Format("Z) {0:000.000000}u/s -> {1:000.000000}u/s | {2:000.000000}u : {3:000.000000}u/s² | {4:000.000000}u/s² | {5:000.000000}u/s²",
            previousSensorVelocity.z,
            currentSensorVelocity.z,
            deltaVelocity.z,
            acceleration.z,
            gravity.z,
            _accelerationFelt.z
        ));

        //print(
        //    String.Format("{0:0.000000}u/s -> {1:0.000000}u/s | {2:0.000000}u : {3:0.000000}u/s² | {4:0.000000}u/s² | {5:0.000000}u/s²",
        //    previousSensorVelocity.y,
        //    currentSensorVelocity.y,
        //    deltaVelocity.y,
        //    acceleration.y,
        //    gravity.y,
        //    accelerationFelt.y
        //));
    }

    #endregion

    #region Projection sur les axes

    /// <summary>
    /// Retourne la valeur d'un vecteur sur un axe
    /// </summary>
    /// <param name="vector">Le vecteur</param>
    /// <param name="normal">L'axe</param>
    /// <returns>La valeur du mouvement sur cet axe</returns>
    private float ProjectMoveAxis(Vector3 vector, Vector3 normal)
    {
        var projected = Vector3.Project(vector, normal);
        return (projected.normalized == normal) ? projected.magnitude : -projected.magnitude;
    }

    /// <summary>
    /// Projette un vecteur sur un autre correspondant à l'orientation du target.
    /// </summary>
    /// <param name="vector">Le vecteur</param>
    /// <param name="target">La transformation du vecteur de résultat</param>
    /// <returns>Le vecteur projeté selon la transform</returns>
    private Vector3 ProjectMove(Vector3 vector, Transform target)
    {
        return new Vector3(
            ProjectMoveAxis(vector, target.right),
            ProjectMoveAxis(vector, target.up),
            ProjectMoveAxis(vector, target.forward)
        );
    }

    #endregion
}
