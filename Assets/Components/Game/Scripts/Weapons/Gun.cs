using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GunData gunData;
    public GameObject bulletPrefab;

    public Transform arm;
    public Transform firePoint;

    public bool flipWithPlayer = false;

    private Camera mainCam;
    private SpriteRenderer armRenderer;
    private SpriteRenderer weaponRenderer;

    private float nextTimeToFire = 0f;
    protected bool isFiring = false;

    private void Start()
    {
        mainCam = Camera.main;
        if (arm != null)
            armRenderer = arm.GetComponent<SpriteRenderer>();

        weaponRenderer = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        AimAtMouse();

        if (Input.GetButton("Fire1"))
        {
            isFiring = true;
            TryShoot();
        }
        else
        {
            isFiring = false;
        }
    }

    private void TryShoot()
    {

        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + gunData.fireRate;
            Shoot();
        }
    }


    private void Shoot()
    {

        // PlaySound
        if(gunData.shootSound != "")
        {
            AudioManager.Instance.PlaySound(gunData.shootSound);
        }
        else
        {
            AudioManager.Instance.PlaySound("GunFire");

        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bullet.AddComponent<Bullet>();
        }
            
        bulletScript.Initialize(gunData);
        Destroy(bullet, 3f); 


    }

    void AimAtMouse()
    {
        // Obtenir la position de la souris dans le monde
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // Calculer la direction de la souris
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Appliquer la rotation
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Le bras suit l’arme
        if (arm != null)
        {
            arm.rotation = Quaternion.Euler(0, 0, angle);
            //armRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Gérer le flip gauche/droite
        if (flipWithPlayer)
        {
            bool facingLeft = mousePos.x < transform.position.x;

            if (facingLeft)
            {
                arm.localScale = new Vector3(1, -1, 1);  // flip visuel du bras
                weaponRenderer.flipY = true;             // retourne le sprite de l’arme
            }
            else
            {
                arm.localScale = new Vector3(1, 1, 1);
                weaponRenderer.flipY = false;
            }
        }
    }

}
