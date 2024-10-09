// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Handles all the animation events that come from the weapon in the asset.
    /// </summary>
    public class WeaponAnimationEventHandler : MonoBehaviour
    {
        #region FIELDS

        /// <summary>
        /// Equipped Weapon.
        /// </summary>
        private WeaponBehaviour weapon;

        #endregion

        #region UNITY

        private void Awake()
        {
            //Cache. We use this one to call things on the weapon later.
            weapon = GetComponent<WeaponBehaviour>();
        }

        #endregion

        #region ANIMATION

        /// <summary>
        /// Ejects a casing from this weapon. This function is called from an Animation Event.
        /// </summary>
        private void OnEjectCasing()
        {
            // Notify to eject casing if the weapon is not null.
            if (weapon != null)
                weapon.EjectCasing();
        }

        /// <summary>
        /// Fires the weapon. Called from an Animation Event.
        /// </summary>
        private void OnFire()
        {
            // Call the fire method on the weapon.
            if (weapon != null)
                weapon.Fire();
        }

        /// <summary>
        /// Reloads the weapon. Called from an Animation Event.
        /// </summary>
        private void OnReload()
        {
            // Call the reload method on the weapon.
            if (weapon != null)
                weapon.Reload();
        }

        #endregion
    }
}
