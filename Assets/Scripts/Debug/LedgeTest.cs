﻿using UnityEngine;
using System.Collections.Generic;
using Common;

public class LedgeTest : MonoBehaviour
{
    public GameObject ClimbUpArrow;
    public GameObject ClimbDownArrow;
    public GameObject KeepUpArrow;
    public GameObject ForceFallArrow;

    private GroundedLedgeBehaviour _ledgeSensor;
    private List<Object> _gameObjects;

    protected void Start()
    {
        _ledgeSensor = GetComponent<GroundedLedgeBehaviour>();
        _gameObjects = new List<Object>();
    }

    protected void Update()
    {
        foreach (var gameObject in _gameObjects)
        {
            Destroy(gameObject);
        }
        _gameObjects.Clear();

        // draw the climb up arrows
        foreach (var upperLedge in _ledgeSensor.UpperLedges)
        {
            if (!upperLedge.GrabPosition.IsInFront)
            {
                var skipIt = true;
                foreach (var ledge in _ledgeSensor.UpperLedges)
                {
                    if (ledge == upperLedge)
                        continue;

                    if (!upperLedge.GrabPosition.Ledge.IsConnectedTo(ledge.GrabPosition.Ledge, 0.02F))
                        continue;

                    skipIt = false;
                    break;
                }

                if (skipIt)
                    continue;
            }

            var arrow = Instantiate(
                ClimbUpArrow, 
                upperLedge.GrabPosition.Value, 
                Quaternion.LookRotation(upperLedge.GrabPosition.PerpendicularGrabDirection)
            );
            _gameObjects.Add(arrow);
        }

        // draw the climb down arrow
        foreach (var lowerLedge in _ledgeSensor.LowerLedges)
        {
            if (!lowerLedge.GrabPosition.IsInFront)
            {
                var skipIt = true;
                foreach (var ledge in _ledgeSensor.LowerLedges)
                {
                    if (ledge == lowerLedge)
                        continue;

                    if (!lowerLedge.GrabPosition.Ledge.IsConnectedTo(ledge.GrabPosition.Ledge, 0.02F))
                        continue;

                    skipIt = false;
                    break;
                }

                if (skipIt)
                    continue;
            }

            Object prefab;
            if (lowerLedge.UpPosition.HasValue)
            {
                if (lowerLedge.DownPosition.HasValue)
                {
                    prefab = ClimbDownArrow;
                }
                else
                {
                    prefab = KeepUpArrow;
                }
            }
            else
            {
                if (lowerLedge.DownPosition.HasValue)
                {
                    prefab = ForceFallArrow;
                }
                else
                {
                    // nothing
                    continue;
                }
            }

            var direction = lowerLedge.IsGrounded ? -lowerLedge.GrabPosition.PerpendicularGrabDirection : lowerLedge.GrabPosition.PerpendicularGrabDirection;

            var arrow = Instantiate(
                prefab,
                lowerLedge.GrabPosition.Value,
                Quaternion.LookRotation(direction)
            );
            _gameObjects.Add(arrow);
        }
    }
}
