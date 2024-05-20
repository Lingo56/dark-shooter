using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastShoot : MonoBehaviour
{
    [SerializeField] private int gunDamage = 1;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float weaponRange = 50f;
    [SerializeField] private float hitForce = 100f;
    [SerializeField] private int numberOfBullets = 10;
    [SerializeField] private float spreadAngle = 10f;
    [SerializeField] private Transform gunBarrelExit;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject muzzleLight;
    [SerializeField] private ParticleSystem bulletImpact;

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
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            StartCoroutine(ShotEffect());

            for (int i = 0; i < numberOfBullets; i++)
            {
                Vector3 spread = CalculateSpread(fpsCam.transform.forward, spreadAngle);
                ShootRay(fpsCam.transform.position, spread);
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
}
