using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastShoot : MonoBehaviour
{
    [Header("Weapon Settings")]
    //[SerializeField] private int gunDamage = 1;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float weaponRange = 50f;
    [SerializeField] private float hitForce = 100f;
    [SerializeField] private int numberOfBullets = 10;
    [SerializeField] private float spreadAngle = 10f;
    [SerializeField] private Transform gunBarrelExit;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject muzzleLight;
    [SerializeField] private ParticleSystem bulletImpact;

    [Header("Kickback Settings")]
    [SerializeField] private float kickbackAmount = 0.1f;
    [SerializeField] private float kickbackSpeed = 5f;
    [SerializeField] private float resetDelay = 0.1f; // Delay before resetting after kickback

    [Header("Shake Settings")]
    [SerializeField] private float shakeAmount = 0.1f; // Amount to shake
    [SerializeField] private float shakeDuration = 0.1f; // Duration of shake

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Camera fpsCam;
    private WaitForSeconds shotDuration = new WaitForSeconds(.5f);
    private AudioSource gunAudio;
    private WFX_LightFlicker wfxLightScript;
    private float nextFire;

    void Start()
    {
        gunAudio = GetComponent<AudioSource>();
        fpsCam = GetComponentInParent<Camera>();
        wfxLightScript = muzzleLight.GetComponent<WFX_LightFlicker>();
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            StartCoroutine(ShotEffect());
            StartCoroutine(KickbackAndReset());
            StartCoroutine(ShakeCamera());

            for (int i = 0; i < numberOfBullets; i++)
            {
                Vector3 spread = CalculateSpread(fpsCam.transform.forward, spreadAngle);
                ShootRay(fpsCam.transform.position, spread);
            }

            // Apply the accumulated force to all enemies hit
            MainEnemyMovement[] enemies = FindObjectsOfType<MainEnemyMovement>();
            foreach (var enemy in enemies)
            {
                enemy.ApplyAccumulatedForce();
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

    private void ShootRay(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, weaponRange))
        {
            Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * hitForce);
            }

            MainEnemyMovement enemyMovement = hit.collider.GetComponent<MainEnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.ApplyHitNormal(hit.normal);
            }
        }
    }

    private IEnumerator ShotEffect()
    {
        gunAudio.Play();
        muzzleFlash.Play();
        handleLight();

        yield return shotDuration;
    }

    private void handleLight()
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
        // Get the global up direction in local space
        Vector3 localUp = transform.InverseTransformDirection(Vector3.up);

        // Kickback
        transform.localPosition -= localUp * kickbackAmount;

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

    private IEnumerator ShakeCamera()
    {
        Quaternion originalCamRotation = fpsCam.transform.localRotation;
        Quaternion targetRotation = originalCamRotation * Quaternion.Euler(-shakeAmount, 0, 0);

        float elapsed = 0.0f;

        // Shake the camera
        while (elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration; // Calculate interpolation parameter
            fpsCam.transform.localRotation = Quaternion.Lerp(originalCamRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is the target rotation
        fpsCam.transform.localRotation = targetRotation;

        // Reset the elapsed time and rotation
        elapsed = 0.0f;
        Quaternion newCameraRotation = fpsCam.transform.localRotation;

        // Smoothly interpolate back to original rotation
        while (elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration; // Calculate interpolation parameter
            fpsCam.transform.localRotation = Quaternion.Lerp(newCameraRotation, originalCamRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exactly the original one
        fpsCam.transform.localRotation = originalCamRotation;
    }

}