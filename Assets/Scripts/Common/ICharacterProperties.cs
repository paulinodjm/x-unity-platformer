using UnityEngine;
using System.Collections;

namespace Common
{
    /// <summary>
    /// Définis les propriétés pour retrouver les propriétés de taille d'un personnage
    /// </summary>
    public interface ICharacterProperties
    {
        /// <summary>
        /// Retourne la hauteur du personnage
        /// </summary>
        float Height { get; }

        /// <summary>
        /// Retourne le rayon du personnage
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// Retourne la hauteur de marche que le personnage peut franchir sans utiliser le mécanisme d'escalade
        /// </summary>
        float StepOffset { get; }
    }
}