using System.Collections;
using System.Collections.Generic;
using Imports.JMO_Assets.WarFX.Scripts;
using UnityEngine;

// TODO: Simplify and improve this script
// TODO: Fix this from firing when the game is over
namespace Actors.Player
{
    public class PlayerRaycastShoot : MonoBehaviour
    {
        [Header("Dependancies")]
        [SerializeField] private GameObject player;

        [SerializeField] private GameObject bulletTrailPrefab;

        [Header("Weapon Settings")]
        [SerializeField] private int gunDamage = 2;

        [SerializeField] private float fireRate = 0.2f;
        [SerializeField] private float weaponRange = 50f;
        [SerializeField] private float bulletHitForce = 1f; // The force of each individual bullet
        [SerializeField] private int numberOfBullets = 10;
        [SerializeField] private float spreadAngle = 10f;
        [SerializeField] private float bulletTrailDuration = 0.1f;
        [SerializeField] private Transform gunBarrelExit;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private GameObject muzzleLight;
        [SerializeField] private ParticleSystem defaultBulletImpact;

        [Header("Kickback Animation Settings")]
        [SerializeField] private float kickbackAmount = 0.1f;

        [SerializeField] private float kickbackSpeed = 5f;
        [SerializeField] private float resetDelay = 0.1f; // Delay before resetting after kickback

        [Header("Screen Shake Settings")]
        [SerializeField] private float shakeAmount = 0.1f; // Amount to shake

        [SerializeField] private float shakeRiseDuration = 0.1f; // Duration of shake
        [SerializeField] private float shakeFallDuration = 0.1f; // Duration of shake

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private Camera fpsCam;
        private WaitForSeconds shotDuration = new(.5f);
        private AudioSource gunAudio;
        private WFX_LightFlicker wfxLightScript;
        private float nextFire;

        private PlayerMovementController fpsController;

        private void Start()
        {
            gunAudio = GetComponent<AudioSource>();
            fpsCam = GetComponentInParent<Camera>();

            fpsController = player.GetComponent<PlayerMovementController>();
            wfxLightScript = muzzleLight.GetComponent<WFX_LightFlicker>();
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;

                StartCoroutine(ShotEffect());
                StartCoroutine(KickbackAndReset());
                StartCoroutine(fpsController.ShakeCamera(shakeAmount, shakeRiseDuration, shakeFallDuration));

                HashSet<EnemyMainController> hitEnemies = new();

                for (int i = 0; i < numberOfBullets; i++)
                {
                    Vector3 spread = CalculateSpread(fpsCam.transform.forward, spreadAngle);
                    ShootRay(fpsCam.transform.position, spread, gunDamage, hitEnemies);
                }

                // Apply the accumulated force to all enemies hit
                foreach (var enemy in hitEnemies)
                {
                    enemy.HandleEnemyDamage();
                }
            }
        }

        private Vector3 CalculateSpread(Vector3 direction, float angle)
        {
            float spreadRadius = Mathf.Tan(angle * Mathf.Deg2Rad);
            float randomX = Random.Range(-spreadRadius, spreadRadius);
            float randomY = Random.Range(-spreadRadius, spreadRadius);
            Vector3 spread = new Vector3(randomX, randomY, 1).normalized;

            return fpsCam.transform.TransformDirection(spread);
        }

        private void ShootRay(Vector3 origin, Vector3 direction, int damage, HashSet<EnemyMainController> hitEnemies)
        {
            Vector3 currentOrigin = origin;
            float remainingRange = weaponRange;

            while (remainingRange > 0)
            {
                if (Physics.Raycast(currentOrigin, direction, out RaycastHit hit, remainingRange))
                {
                    // Ignore objects with the "Effect" tag
                    if (hit.collider.CompareTag("Effect"))
                    {
                        // Move the origin of the ray to the point just past the current hit
                        currentOrigin = hit.point + direction * 0.01f; // Small offset to avoid re-hitting the same point
                        remainingRange -= hit.distance;
                        continue; // Continue casting the ray
                    }

                    EnemyMainController enemyController = hit.collider.GetComponent<EnemyMainController>();

                    if (enemyController != null)
                    {
                        enemyController.ProcessHit(damage, bulletHitForce, hit);
                        hitEnemies.Add(enemyController); // Track this enemy as hit
                    }

                    CreateBulletTrail(gunBarrelExit.position, hit.point, hit.point, hit.normal);

                    ParticleSystem impactEffect = Instantiate(defaultBulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
                    impactEffect.transform.SetParent(hit.collider.transform);

                    break; // Stop casting the ray after hitting a valid target
                }
                else
                {
                    // If no hit, create a bullet trail to the max range
                    CreateBulletTrail(gunBarrelExit.position, origin + direction * weaponRange, origin + direction * weaponRange, Vector3.forward);
                    break; // Exit the loop since we've reached the maximum range
                }
            }
        }

        private void CreateBulletTrail(Vector3 start, Vector3 end, Vector3 hitPoint, Vector3 hitNormal)
        {
            GameObject bulletTrail = Instantiate(bulletTrailPrefab, start, Quaternion.identity);
            LineRenderer lineRenderer = bulletTrail.GetComponent<LineRenderer>();

            StartCoroutine(MoveBulletTrail(lineRenderer, start, end, hitPoint, hitNormal));
        }

        private IEnumerator MoveBulletTrail(LineRenderer lineRenderer, Vector3 start, Vector3 end, Vector3 hitPoint, Vector3 hitNormal)
        {
            float elapsedTime = 0f;

            while (elapsedTime < bulletTrailDuration)
            {
                // Calculate interpolation factor (0 to 1)
                float t = elapsedTime / bulletTrailDuration;

                // Lerp the points from start to end
                Vector3 lerpedStartPoint = Vector3.Lerp(start, end, t - 0.25f); // Using t * t for smoother movement
                Vector3 lerpedEndPoint = Vector3.Lerp(start, end, t);

                // Update the positions of the LineRenderer
                lineRenderer.SetPosition(0, lerpedStartPoint);
                lineRenderer.SetPosition(1, lerpedEndPoint);

                // Increment elapsed time
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Deactivate or destroy the GameObject containing the LineRenderer
            Destroy(lineRenderer.gameObject);
        }

        private IEnumerator ShotEffect()
        {
            gunAudio.Play();
            muzzleFlash.Play();
            HandleLight();

            yield return shotDuration;
        }

        private void HandleLight()
        {
            if (!muzzleLight.activeSelf)
            {
                muzzleLight.SetActive(true);
            }
            else
            {
                if (wfxLightScript != null)
                {
                    wfxLightScript.ResetTimer();
                }
            }
        }

        private IEnumerator KickbackAndReset()
        {
            // Get the local backward direction along the Z axis
            Vector3 localBackward = new Vector3(0, 0, -1);

            // Kickback
            transform.localPosition += localBackward * kickbackAmount;

            // Wait for reset delay
            yield return new WaitForSeconds(resetDelay);

            // Smoothly reset the position and rotation to initial state
            while (Vector3.Distance(transform.localPosition, initialPosition) > 0.001f ||
                   Quaternion.Angle(transform.localRotation, initialRotation) > 0.001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * kickbackSpeed);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation, Time.deltaTime * kickbackSpeed);
                yield return null;
            }

            // Ensure final position and rotation are exactly the initial ones
            transform.localPosition = initialPosition;
            transform.localRotation = initialRotation;
        }
    }
}