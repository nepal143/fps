using TMPro;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Firing")]

        [Tooltip("Is this weapon automatic? If yes, then holding down the firing button will continuously fire.")]
        [SerializeField]
        private bool automatic;

        [Tooltip("How fast the projectiles are.")]
        [SerializeField]
        private float projectileImpulse = 400.0f;

        [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
        [SerializeField]
        private int roundsPerMinutes = 200;

        [Tooltip("Mask of things recognized when firing.")]
        [SerializeField]
        private LayerMask mask;

        [Tooltip("Maximum distance at which this weapon can fire accurately. Shots beyond this distance will not use linetracing for accuracy.")]
        [SerializeField]
        private float maximumDistance = 500.0f;

        [Header("Animation")]

        [Tooltip("Transform that represents the weapon's ejection port, meaning the part of the weapon that casings shoot from.")]
        [SerializeField]
        private Transform socketEjection;

        [Header("Resources")]

        [Tooltip("Casing Prefab.")]
        [SerializeField]
        private GameObject prefabCasing;

        [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")]
        [SerializeField]
        private GameObject prefabProjectile;

        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")]
        [SerializeField]
        public RuntimeAnimatorController controller;

        [Tooltip("Weapon Body Texture.")]
        [SerializeField]
        private Sprite spriteBody;

        [Header("Audio Clips Holster")]

        [Tooltip("Holster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipHolster;

        [Tooltip("Unholster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipUnholster;

        [Header("Audio Clips Reloads")]

        [Tooltip("Reload Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReload;

        [Tooltip("Reload Empty Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadEmpty;

        [Header("Audio Clips Other")]

        [Tooltip("AudioClip played when this weapon is fired without any ammunition.")]
        [SerializeField]
        private AudioClip audioClipFireEmpty;

        [Header("UI")]
        [SerializeField] private TMP_Text ammunitionCurrentText;
        [SerializeField] private TMP_Text totalBulletsText;

        #endregion

        #region FIELDS

        private Animator animator;
        private WeaponAttachmentManagerBehaviour attachmentManager;
        private int ammunitionCurrent;
        private int totalBullets = 30;

        private MagazineBehaviour magazineBehaviour;
        private MuzzleBehaviour muzzleBehaviour;
        private IGameModeService gameModeService;
        private CharacterBehaviour characterBehaviour;
        private Transform playerCamera;

        #endregion

        #region UNITY

        protected override void Awake()
        {
            // Get Animator.
            animator = GetComponent<Animator>();
            // Get Attachment Manager.
            attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();

            // Cache the game mode service.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            // Cache the player character.
            characterBehaviour = gameModeService.GetPlayerCharacter();
            // Cache the world camera.
            playerCamera = characterBehaviour.GetCameraWorld().transform;

            // Update ammo UI at the start
            UpdateAmmoUI();
        }

        protected override void Start()
        {
            // Get Magazine and Muzzle.
            magazineBehaviour = attachmentManager.GetEquippedMagazine();
            muzzleBehaviour = attachmentManager.GetEquippedMuzzle();

            // Max Out Ammo.
            ammunitionCurrent = magazineBehaviour.GetAmmunitionTotal();
            UpdateAmmoUI();
        }

        #endregion

        #region GETTERS

        public override Animator GetAnimator() => animator;
        public override Sprite GetSpriteBody() => spriteBody;
        public override AudioClip GetAudioClipHolster() => audioClipHolster;
        public override AudioClip GetAudioClipUnholster() => audioClipUnholster;
        public override AudioClip GetAudioClipReload() => audioClipReload;
        public override AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;
        public override AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;
        public override AudioClip GetAudioClipFire() => muzzleBehaviour.GetAudioClipFire();
        public override int GetAmmunitionCurrent() => ammunitionCurrent;
        public override int GetAmmunitionTotal() => magazineBehaviour.GetAmmunitionTotal();
        public override bool IsAutomatic() => automatic;
        public override float GetRateOfFire() => roundsPerMinutes;
        public override bool IsFull() => ammunitionCurrent == magazineBehaviour.GetAmmunitionTotal();
        public override bool HasAmmunition() => ammunitionCurrent > 0;
        public override RuntimeAnimatorController GetAnimatorController() => controller;
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => attachmentManager;

        #endregion

        #region METHODS

        public override void Reload()
        {
            // Check if there's ammunition left in total bullets and the weapon isn't full.
            if (totalBullets > 0 && !IsFull())
            {
                // Calculate the amount of ammo we can reload.
                int missingAmmo = magazineBehaviour.GetAmmunitionTotal() - ammunitionCurrent;
                int reloadAmount = Mathf.Min(missingAmmo, totalBullets);

                // Only reload if there's a need to do so.
                if (reloadAmount > 0)
                {
                    // Subtract from total bullets.
                    totalBullets -= reloadAmount;
                    // Add to current ammo.
                    ammunitionCurrent += reloadAmount;

                    // Play reload animation.
                    animator.Play(HasAmmunition() ? "Reload" : "Reload Empty", 0, 0.0f);

                    // Update ammo UI.
                    UpdateAmmoUI();
                }
            }
            else if (totalBullets == 0)
            {
                Debug.Log("No bullets left to reload.");
            }
        }

        public override void Fire(float spreadMultiplier = 1.0f)
        {
            if (muzzleBehaviour == null || playerCamera == null)
                return;

            if (!HasAmmunition())
            {
                AudioSource.PlayClipAtPoint(audioClipFireEmpty, transform.position);
                return;
            }

            Transform muzzleSocket = muzzleBehaviour.GetSocket();
            animator.Play("Fire", 0, 0.0f);
            ammunitionCurrent = Mathf.Clamp(ammunitionCurrent - 1, 0, magazineBehaviour.GetAmmunitionTotal());

            muzzleBehaviour.Effect();

            Quaternion rotation = Quaternion.LookRotation(playerCamera.forward * 1000.0f - muzzleSocket.position);

            if (Physics.Raycast(new Ray(playerCamera.position, playerCamera.forward), out RaycastHit hit, maximumDistance, mask))
                rotation = Quaternion.LookRotation(hit.point - muzzleSocket.position);

            GameObject projectile = Instantiate(prefabProjectile, muzzleSocket.position, rotation);
            projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileImpulse;

            // Update ammo UI.
            UpdateAmmoUI();
        }

        public override void FillAmmunition(int amount)
        {
            totalBullets = amount;
            UpdateAmmoUI();
        }

        public override void EjectCasing()
        {
            if (prefabCasing != null && socketEjection != null)
                Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
        }

        private void UpdateAmmoUI()
        {
            if (ammunitionCurrentText != null)
                ammunitionCurrentText.text = ammunitionCurrent.ToString();

            if (totalBulletsText != null)
                totalBulletsText.text = totalBullets.ToString();
        }

        #endregion
    }
}
