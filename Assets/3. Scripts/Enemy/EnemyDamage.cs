using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDamage : MonoBehaviour
{
    private const string bulletTag = "Bullet";
    private float hp = 100.0f; //체력 게이지
    private float initHp = 100.0f; //초기 체력
    private GameObject bloodEffect; //피격시 사용할 효과 

    [HideInInspector]public bool getShot = false; //EnemyAI 피격 확인용

    void Start()
    {
        bloodEffect = Resources.Load<GameObject>("Effect/BulletImpactFleshBigEffect");
    }


    void OnCollisionEnter(Collision coll)
    {
        if(coll.collider.tag == bulletTag)
        {
            ShowBloodEffect(coll); //이펙트 생성 
            Destroy(coll.gameObject); //총알 삭제
            getShot = true;

            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage; //생명게이지를 총알의 데미지만큼 차감

            if (hp <= 0.0f)
            {
                GetComponent<EnemyAI>().state = EnemyAI.State.DIE; //적 상태를 사망으로 변경
                GameManager.instance.IncKillCount(); //killcount
                GetComponent<CapsuleCollider>().enabled = false; //캡슐콜라이더 비활성화
                Destroy(gameObject,5.0f); //5초 후 삭제 
            }
        }
    }

    void ShowBloodEffect(Collision coll)
    {
        Vector3 pos = coll.contacts[0].point; //총알 충돌 지점
        Vector3 _normal = coll.contacts[0].normal; //총알 충돌시 법선 벡터
        Quaternion rot = Quaternion.FromToRotation(-Vector3.forward, _normal); //총알 충돌 시 방향 벡터의 회전값 계산 

        GameObject blood = Instantiate<GameObject>(bloodEffect, pos, rot); //혈흔 효과 생성 
        Destroy(blood, 1.0f);
    }

}
