using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OVRgunCtrl : MonoBehaviour
{
    private float coolFire = 0;
    private float coolReload = 0;

    private AudioSource audio;
    private Animator anim;

    [Header("Ammo Info")]
    public GameObject Bullet;
    public Transform firePos;
    public Text text1, text2;
    public int maxAmmo = 15;
    [HideInInspector] public int curAmmo;

    [Header("SFX Info")]
    public AudioClip fireSfx;
    public AudioClip reloadSfx;
    public AudioClip noAmmoSfx;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        curAmmo = maxAmmo;
        ammoPrint();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (curAmmo > 0 && (coolFire<Time.time))
            {
                Fire();
            }
            else if(curAmmo == 0)
                audio.PlayOneShot(noAmmoSfx);

        }

        if (Vector3.Angle(transform.up, Vector3.up) > 70 && curAmmo < maxAmmo && coolReload < Time.time)
        { 
            Reload();
         }
    }

    void Fire()
    {
            anim.SetTrigger("Fire");
            audio.PlayOneShot(fireSfx);
            Instantiate(Bullet, firePos.position, firePos.rotation);
            curAmmo--;
            ammoPrint();
            coolFire = Time.time + 0.4f;
    }

    void Reload()
    {
        anim.SetTrigger("Reload");
        curAmmo = maxAmmo;
        audio.PlayOneShot(reloadSfx);
        ammoPrint();
        coolReload = Time.time + 2.0f;
    }

    void ammoPrint()
    {
        text1.text = curAmmo.ToString();
        text2.text = curAmmo.ToString();
    }
}
