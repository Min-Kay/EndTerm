using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCtrl : MonoBehaviour
{
    [Header("Barrel Info")]
    public GameObject expEffect;
    public Mesh[] meshes; //찌그러진 드럼통의 메쉬 저장 배열
    public Texture[] textures; // 드럼통 텍스처 저장 배열
    public float expRadius = 7.0f; //폭발 반경   
    public AudioClip expSfx;

    private int hitCount = 0;
    private Rigidbody rb;
    private MeshFilter meshFilter;
    private MeshRenderer _renderer;
    private AudioSource _audio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _audio = GetComponent<AudioSource>();

        //난수 발생으로 불규칙적 텍스처 적용
        _renderer.material.mainTexture= textures[Random.Range(0, textures.Length)];
    }

    private void OnCollisionEnter(Collision coll) //충돌 판정
    {
        if(coll.collider.CompareTag("Bullet"))
        {
            if(++hitCount == 3) //3회 충돌시 폭발
            {
                ExpBarrel();
            }
        }
    }

    void ExpBarrel() //폭발 효과 처리 함수
    {
        GameObject effect = Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(effect, 2.0f);

        IndirectDamage(transform.position);

        _audio.PlayOneShot(expSfx, 2.0f); //폭발음 발생

        //찌그러진 모형 적용을 위한 난수 발생
        int idx = Random.Range(0, meshes.Length);
        meshFilter.sharedMesh = meshes[idx]; //찌그러진 메쉬 적용
        Destroy(gameObject, 5.0f);
    }

    void IndirectDamage(Vector3 pos) //반경내 폭발 스플래쉬
    {
        Collider[] colls = Physics.OverlapSphere(pos, expRadius, 1 << 11);

        foreach(var coll in colls)
        {
            var _rb = coll.GetComponent<Rigidbody>();
            _rb.mass = 1.0f;
            _rb.AddExplosionForce(1200.0f, pos, expRadius, 1000.0f);
        }
    }
}
