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
    [SerializeField] private GameObject bulletImpact;

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

            Vector3 rayOrigin = fpsCam.transform.position;

            for (int i = 0; i < numberOfBullets; i++)
            {
                // Calculate spread for each bullet
                Vector3 spread = fpsCam.transform.forward;
                spread += new Vector3(
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle)
                ).normalized * 0.1f;

                RaycastHit hit;

                if (Physics.Raycast(rayOrigin, spread, out hit, weaponRange))
                {
                    Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));

                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForce(-hit.normal * hitForce);
                    }
                }
            }
        }
    }

    private IEnumerator ShotEffect()
    {
        gunAudio.Play();
        muzzleFlash.Play();

        //handleLight();

        yield return shotDuration;
    }

    private void handleLight()
    {
        if (!muzzleLight.activeSelf) {
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
