using UnityEngine;
using System.Collections;

namespace Common
{
    public interface IAnimatorController
    {
        /// <summary>
        /// Assigne la valeur actuelle de l'input pour le déplacement
        /// </summary>
        float InputMove
        {
            set;
        }

        /// <summary>
        /// Retourne la gravité à appliquer à la vélocité actuelle
        /// </summary>
        float Gravity
        {
            get;
        }

        /// <summary>
        /// Retourne ou assigne la vélocité du personnage, dans un repère local
        /// </summary>
        Vector3 Velocity
        {
            get;
            set;
        }

        /// <summary>
        /// Retourne le facteur d'interpolation de l'orientation du personnage en fonction de son déplacement
        /// </summary>
        float RotationLerpFactor
        {
            get;
        }
    }
}