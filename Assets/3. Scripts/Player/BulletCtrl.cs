using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    [Header("Bullet Data Info")]
    public float damage = 20.0f;
    public float speed = 1000.0f;

    void Start() 
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * speed);
        Destroy(gameObject,3.0f);
    }

}
