using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OculusSampleFramework
{
    public class GrapItem : MonoBehaviour
    {
        private const string HealItem = "HealBlock";
        private const string SpeedItem = "SpeedBlock";
        private const string DamageItem = "DamageBlock";

        [Header("Item Target Info")]
        public BulletCtrl _bullet;
        public OVRPlayerController player;
       
        private DistanceGrabber distanceGrabber;
        private DistanceGrabbable item;
        private Damage playerHp;

        [Header("Item Spec Info")]
        public float Heal = 30.0f;
        public float AdditionalDamage = 5.0f;
        public float Speed = 0.05f;

        private float maxDamage = 40.0f;
        private float maxSpeed = 0.5f;

        private float addHeal;
        private float addDamage;
        private float addSpeed;

        private AudioSource audio;

        [Header("Item SFX Info")]
        public AudioClip healSfx;
        public AudioClip speedSfx;
        public AudioClip damageSfx;

        private void Start()
        {
            playerHp = player.GetComponentInChildren<Damage>();
            distanceGrabber = GetComponent<DistanceGrabber>();
            audio = GetComponent<AudioSource>();
        }

        void Update()
        {
            item = (DistanceGrabbable)distanceGrabber.grabbedObject;

            if (item.grabbedBy == distanceGrabber && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            {
                CheckItemTag();
            }
        }

        public void CheckItemTag()
        {
            if (item.tag == HealItem)
                HealthUp();
            else if (item.tag == SpeedItem)
                SpeedUp();
            else if (item.tag == DamageItem)
                DamageUp();
        }


        public void HealthUp()
        {
            if ((playerHp.currHP + Heal) <= playerHp.initHP)
            {
                playerHp.currHP += Heal;
                GameManager.instance.addHeal = Heal;
                GameManager.instance.isHeal = true;
                playerHp.CheckHp();
                audio.PlayOneShot(healSfx);
                Destroy(item.gameObject);
            }
            else if (playerHp.currHP == playerHp.initHP)
            {
                GameManager.instance.isFullHp = true;
            }
            else if ((playerHp.currHP + Heal) > playerHp.initHP && playerHp.currHP < playerHp.initHP)
            {
                addHeal = playerHp.initHP - playerHp.currHP;
                playerHp.currHP = playerHp.initHP;
                GameManager.instance.addHeal = addHeal;
                GameManager.instance.isHeal = true;
                playerHp.CheckHp();
                audio.PlayOneShot(healSfx);
                Destroy(item.gameObject);
            }
        }

        public void SpeedUp()
        {
            if (player.Acceleration < maxSpeed)
            {
                if (player.Acceleration + Speed <= maxSpeed)
                {
                    player.Acceleration += Speed;
                    addSpeed = Speed;
                }
                else
                {
                    player.Acceleration = maxSpeed;
                    addSpeed = (player.Acceleration + Speed) - maxSpeed;
                }
                GameManager.instance.addSpeed = addSpeed;
                GameManager.instance.speedAmount = player.Acceleration;
                GameManager.instance.isSpeed = true;
                player.Acceleration += Speed;
                audio.PlayOneShot(speedSfx);
                Destroy(item.gameObject);
            }
            else
            {
                GameManager.instance.isFullSp = true;
                Destroy(item.gameObject);
            }
        }

        public void DamageUp()
        {
            if (_bullet.damage < maxDamage)
            {
                if ((_bullet.damage + AdditionalDamage) <= maxDamage)
                {
                    _bullet.damage += AdditionalDamage;
                    addDamage = AdditionalDamage;
                }
                else
                {
                    _bullet.damage = maxDamage;
                    addDamage = (_bullet.damage + AdditionalDamage) - maxDamage;
                }

                GameManager.instance.addDamage = addDamage;
                GameManager.instance.damageAmount = _bullet.damage;
                GameManager.instance.isDamage = true;
                audio.PlayOneShot(damageSfx);
                Destroy(item.gameObject);
            }
            else
            {
                GameManager.instance.isFullDm = true;
                Destroy(item.gameObject);
            }

           
        }
    }
}
