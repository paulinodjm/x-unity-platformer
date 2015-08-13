using UnityEngine;
using System.Collections.Generic;
using Common;

[RequireComponent(typeof(LedgeSensor))]
[RequireComponent(typeof(ICharacterProperties))]
public class GroundedLedgeBehaviour : MonoBehaviour
{
    #region Inspector

    [Tooltip("The horizontal margin kept between the character and the ledge; prevend collision check errors")]
    public float Margin = 0.02F;

    public Color UpperLedgeColor = Color.blue;
    public Color LowerLedgeColor = Color.black;

    public List<Ledge> UpperLedges;
    public List<Ledge> LowerLedges;

    #endregion

    private LedgeSensor _ledgeSensor;
    private ICharacterProperties _character;

    protected void Start()
    {
        _ledgeSensor = GetComponent<LedgeSensor>();
        _character = GetComponent<ICharacterProperties>();
    }

    protected void Update()
    {
        UpperLedges.Clear();
        LowerLedges.Clear();

        foreach (var ledge in _ledgeSensor.Ledges)
        {
            var grabPosition = ledge.CalcGrabPosition(transform.position, _character.Radius + Margin);
            var deltaHeight = grabPosition.Value.y - transform.position.y;

            if (deltaHeight > _character.StepOffset)
            {
                Debug.DrawRay(grabPosition.Value, Vector3.up, UpperLedgeColor);
                UpperLedges.Add(ledge);
            }
            else if (deltaHeight >= -_character.StepOffset)
            {
                Debug.DrawRay(grabPosition.Value, Vector3.up, LowerLedgeColor);
                LowerLedges.Add(ledge);
            }
            else
            {
                // discard this ledge
            }
        }
    }
}
