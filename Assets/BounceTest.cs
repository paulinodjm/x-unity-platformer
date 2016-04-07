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

    void Start()
    {
        if (Sensor == null)
        {
            Debug.LogError("No anchor");
            enabled = false;
        }

        _previousSensorPosition = Sensor.position;
    }

	void Update ()
    {
        transform.Translate(Vector3.up * Input.GetAxis("Vertical") * Speed * Time.deltaTime);
        transform.Rotate(Vector3.left, Input.GetAxis("Horizontal") * RotationSpeed * Time.deltaTime);

        DetectMove();
	}

    void OnDrawGizmos()
    {
        if (Sensor == null)
            return;

        Gizmos.matrix = Matrix4x4.TRS(Sensor.position, Sensor.rotation, Vector3.one);

        // affiche les axes
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.down*2, Vector3.up*2);
        Gizmos.DrawCube(Vector3.up, Vector3.one / 10f);
        Gizmos.DrawCube(Vector3.down, Vector3.one / 10f);

        Gizmos.DrawLine(Vector3.left, Vector3.right);
        Gizmos.DrawLine(Vector3.back, Vector3.forward);

        // affiche la bouboule
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(SensorVelocity, .15f);
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
}
