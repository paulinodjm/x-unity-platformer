using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Moves the charater's hairs regarding his speed
/// </summary>
public class HairMover : MonoBehaviour
{
    public float Multiplier = 1;
    public List<Transform> Hairs;

    private CharacterController _characterController;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (_characterController.velocity != Vector3.zero)
        {
            var force = -_characterController.velocity * Multiplier;

            foreach (var hair in Hairs)
            {
                if (hair == null) continue;

                hair.gameObject.GetComponent<Rigidbody>().AddForce(force);
            }
        }
    }
}
