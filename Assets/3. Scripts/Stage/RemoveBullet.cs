using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBullet : MonoBehaviour
{
    public GameObject sparkEffect;

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.tag == "Bullet")
        {
            ShowEffect(coll);
            Destroy(coll.gameObject);
        }
    }

    void ShowEffect(Collision coll)
    {
        ContactPoint contact = coll.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(-Vector3.forward,contact.normal);

        GameObject spark = Instantiate(sparkEffect, contact.point + (-contact.normal * 0.05f), rot);
        spark.transform.SetParent(this.transform); //스파크 효과의 부모를 피격개체로 설정
    }
}
