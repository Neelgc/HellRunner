using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Gun/GunData")]
public class GunData : ScriptableObject
{
    public string gunName;

    public float fireRate = 0.5f;

    public float bulletSpeed = 20f;

    public int damage = 25;

    public int penetration = 1;

    public bool useGravity = false;

    public string shootSound = "";

}