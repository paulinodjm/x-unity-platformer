using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour 
{
    private CharacterController _characterController;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
	
	void Update () 
    {
        _characterController.Move(new Vector3(0, 0, 1) * Time.deltaTime);
	}
}
