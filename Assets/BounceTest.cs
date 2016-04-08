using UnityEngine;
using System.Collections;

public class BounceTest : MonoBehaviour
{
    public Transform Sensor;

    public float RotationSpeed;
    public float Speed;
    public float SpeedScale;

    /// <summary>
    /// Retourne le décalage actuel de la base par rapport au senseur
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
    /// Retourne le facteur de gravité influant sur les trois axes
    /// </summary>
    public Vector3 Gravity
    {
        get
        {
            return _gravity;
        }
    }
    private Vector3 _gravity;

    /// <summary>
    /// Retourne le facteur d'attraction simulée (déplacement du sensor + gravité) depuis la dernière update
    /// </summary>
    public Vector3 Attraction
    {
        get
        {
            return _attraction;
        }
    }
    private Vector3 _attraction;

    /// <summary>
    /// Retourne le facteur d'accélération constaté sur l'accélération
    /// </summary>
    public Vector3 Acceleration
    {
        get
        {
            return _acceleration;
        }
    }
    private Vector3 _acceleration;

    void Start()
    {
        if (Sensor == null)
        {
            Debug.LogError("No anchor");
            enabled = false;
        }

        _previousSensorPosition = Sensor.position;
    }

    void Update()
    {
        transform.Translate(Vector3.up * Input.GetAxis("Vertical") * Speed * Time.deltaTime);
        transform.Rotate(Vector3.left, Input.GetAxis("Horizontal") * RotationSpeed * Time.deltaTime);

        DetectMove();
        DetectGravity();
        CalcAttraction();
    }

    void OnDrawGizmos()
    {
        if (Sensor == null)
            return;

        Gizmos.matrix = Matrix4x4.TRS(Sensor.position, Sensor.rotation, Vector3.one);

        // affiche les axes
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.down * 2, Vector3.up * 2);
        Gizmos.DrawCube(Vector3.up, Vector3.one / 10f);
        Gizmos.DrawCube(Vector3.down, Vector3.one / 10f);

        Gizmos.DrawLine(Vector3.left, Vector3.right);
        Gizmos.DrawLine(Vector3.back, Vector3.forward);

        // affiche le capteur de déplacement (trainée)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(SensorVelocity, .085f);

        // affiche le capteur de gravité
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Gravity, .07f);

        // affiche l'indicateur d'attraction
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Attraction, .06f);

        // affiche l'indicateur d'accélération
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(Acceleration, .05f);
    }

    #region Détection des mouvements du capteur

    /// <summary>
    /// Position précédente du sensor
    /// </summary>
    private Vector3 _previousSensorPosition;

    /// <summary>
    /// Détecte les mouvements effectués par le capteur depuis la dernière update,
    /// et met à jour la valeur du <see cref="SensorVelocity"/>
    /// </summary>
    private void DetectMove()
    {
        var rawMove = (_previousSensorPosition - Sensor.position) / Time.deltaTime; // vitesse absolue

        _sensorVelocity.x = ProjectMoveAxis(rawMove, Sensor.right) / SpeedScale;
        _sensorVelocity.y = ProjectMoveAxis(rawMove, Sensor.up) / SpeedScale;
        _sensorVelocity.z = ProjectMoveAxis(rawMove, Sensor.forward) / SpeedScale;

        _previousSensorPosition = Sensor.position;
    }

    /// <summary>
    /// Retourne la valeur d'un déplacement sur un axe
    /// </summary>
    /// <param name="move">Le déplacment</param>
    /// <param name="normal">L'axe</param>
    /// <returns>La valeur du mouvement sur cet axe</returns>
    private float ProjectMoveAxis(Vector3 move, Vector3 normal)
    {
        var projected = Vector3.Project(move, normal);
        return (projected.normalized == normal) ? projected.magnitude : -projected.magnitude;
    }

    #endregion

    #region Détection de l'influence de la gravité

    private void DetectGravity()
    {
        var gravity = Vector3.down;

        _gravity.x = ProjectMoveAxis(gravity, Sensor.right);
        _gravity.y = ProjectMoveAxis(gravity, Sensor.up);
        _gravity.z = ProjectMoveAxis(gravity, Sensor.forward);
    }

    #endregion

    #region Calcul de l'attraction simulée

    /// <summary>
    /// Met à jour l'attraction simulée en fonction des déplacements du capteur
    /// et de la gravité
    /// </summary>
    private void CalcAttraction()
    {
        var previousAttraction = _attraction;
        _attraction = _gravity + _sensorVelocity;
        _acceleration = _attraction - previousAttraction;
    }

    #endregion
}
