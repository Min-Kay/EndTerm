using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossDamage : MonoBehaviour
{
    private const string bulletTag = "Bullet";
    private GameObject bloodEffect; //피격시 사용할 효과 

    [Header("Boss HP Info")]
    public float hp; //체력 게이지
    public float initHp = 3000.0f; //초기 체력

    void Start()
    {
        bloodEffect = Resources.Load<GameObject>("Effect/BulletImpactFleshBigEffect");
        hp = initHp;
        GameManager.instance.bossHp = hp;
        GameManager.instance.IncBossHp();
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.tag == bulletTag)
        {
            ShowBloodEffect(coll); //이펙트 생성 
            Destroy(coll.gameObject); //총알 삭제

            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage; //생명게이지를 총알의 데미지만큼 차감

            GameManager.instance.bossHp = hp;
            GameManager.instance.IncBossHp();

            if (hp <= 0.0f)
            {
                GetComponent<BossAI>().state = BossAI.State.DIE; //적 상태를 사망으로 변경
                GetComponent<BoxCollider>().enabled = false; //캡슐콜라이더 비활성화
                GameManager.instance.isClear = true;
                GameManager.instance.IncKillCount();
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
