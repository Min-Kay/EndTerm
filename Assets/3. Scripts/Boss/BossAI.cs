using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BossAI : MonoBehaviour
{
    public enum State //적 상태 표현 열거형 변수
    {
        BORN,
        STAY,
        TRACE,
        ATTACK,
        DIE
    }

    [HideInInspector]public State state = State.TRACE; //상태 저장

    private Transform playerTr; //플레이어 위치 저장
    private Transform enemyTr; //적 위치 저장 
    private Animator animator; // Animator 컴포넌트 저장 변수
    private AudioSource audio; //오디오 소스
    private Shake shake; 

    [Header("SFX Info")]
    public AudioClip Attack1Sfx; //공격1
    public AudioClip Attack2Sfx; //공격2
    public AudioClip RoarSfx; //포효
    public AudioClip DieSfx; //사망
   

    private float nextAttack = 0; //공격 쿨타임

    [Header("Attack Distance Info")]
    public float attackDist = 4.0f; //공격 사정거리
    
    [Header("Check Info")]
    public bool isDie = false; //사망여부 판단 
    public bool isBorn = true;

    private WaitForSeconds ws; //코루틴에서 사용할 지연시간 변수
    private MoveBoss moveAgent; //이동을 제어하는 MoveAgent 클래스 저장 변수

    //애니메이터 컨트롤러에 정의한 파라미터의 해시값을 미리 추출
    private readonly int hashBorn = Animator.StringToHash("IsBorn");
    private readonly int hashMove = Animator.StringToHash("IsMove");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashAttackIdx = Animator.StringToHash("AttackIdx");

    void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("PLAYER"); //주인공 게임오브젝트 추출
        if (player != null)
            playerTr = player.GetComponent<Transform>(); //주인공 Transform 추출

        enemyTr = GetComponent<Transform>();// 적 Transform 추출
        animator = GetComponent<Animator>();
        moveAgent = GetComponent<MoveBoss>();
        audio = GetComponent<AudioSource>();
        shake = GameObject.Find("OVRPlayerController").GetComponent<Shake>();

        ws = new WaitForSeconds(0.3f); //코루틴 지연시간 생성


    }

    void OnEnable()
    {
        StartCoroutine(CheckState()); //CheckState 코루틴 함수 실행
        StartCoroutine(Action()); //Action 코루틴 함수 실행 

        Damage.OnPlayerDie += this.OnPlayerDie; //+= 는 이벤트 연결
    }

    void OnDisable() //이벤트는 반드시 스크립트 활성화 시점에 연결하고 비활성화될때 해제해야한다.
    {
        Damage.OnPlayerDie -= this.OnPlayerDie; //-=는 이벤트 해지
    }

    IEnumerator CheckState() //적 상태를 검사
    {
        while (!isDie)
        {
            if (state == State.DIE) yield break; //사망시 코루틴 종료

            //주인공과 적 간의 거리 계산
            float dist = Vector3.Distance(playerTr.position, enemyTr.position);

            if(isBorn == true)
            {
                state = State.BORN;
            }
            else if (dist <= attackDist && Time.time >= nextAttack) //공격 사정거리에 들고 공격 쿨타임이 돌았을때
            {
                state = State.ATTACK;
            }
            else if (dist <= attackDist) //공격사정거리 안에 해당하나 공격 쿨타임이 돌지 않을경우
            {
                state = State.STAY;
            }
            else //공격 사정거리 밖인 경우
            {
                state = State.TRACE;
            }
          
            yield return ws; //0.3초 동안 대기하는 동안 제어권 양보
        }
    }

    IEnumerator Action() //상태에 따라 행동처리하는 코루틴
    {
        while (!isDie) //적이 사망하기 전까지 무한루프
        {
            yield return ws;

            switch (state) //상태에 따른 분기
            {
                case State.STAY:
                    moveAgent.Stop();
                    animator.SetBool(hashMove, false);
                    GameManager.instance.isBossAttack = false;
                    break;
                case State.BORN:
                    yield return new WaitForSeconds(0.3f);
                    moveAgent.Stop();
                    animator.SetBool(hashMove, false);
                    animator.SetTrigger(hashBorn); 
                    GetComponent<BoxCollider>().enabled = false;
                    audio.PlayOneShot(RoarSfx, 3.0f);
                    StartCoroutine(shake.ShakeCamera(3.0f,0.4f,0.2f)); 
                    yield return new WaitForSeconds(3.0f);
                    isBorn = false;
                    GetComponent<BoxCollider>().enabled = true;
                    break;
                case State.TRACE:
                    moveAgent.traceTarget = playerTr.position;
                    animator.SetBool(hashMove, true);
                    break;
                case State.ATTACK:
                    moveAgent.Stop();
                    animator.SetBool(hashMove, false);
                    GameManager.instance.isBossAttack = true;
                    animator.SetInteger(hashAttackIdx, UnityEngine.Random.Range(1, 5));
                    AttackSound();
                    animator.SetTrigger(hashAttack);
                    yield return new WaitForSeconds(1.0f);
                    GameManager.instance.isBossAttack = false;
                    nextAttack = Time.time + 1.5f;
                    break;
                case State.DIE:
                    isDie = true;
                    moveAgent.Stop();
                    animator.SetTrigger(hashDie); //사망 애니메이션 실행
                    audio.PlayOneShot(DieSfx, 2.0f);
                    GetComponent<BoxCollider>().enabled = false; //충돌 판정 비활성화
                    break;
            }
        }
    }

    public void OnPlayerDie() //플레이어 사망시 호출됨. Damage 함수 참고 
    {
        moveAgent.Stop();
        StopAllCoroutines(); //모든 코루틴 함수 종료

        animator.SetTrigger(hashPlayerDie);
        audio.PlayOneShot(RoarSfx);
    }

    public void AttackSound() //공격 사운드 분기
    {
        int i = UnityEngine.Random.Range(0, 2);

        switch (i)
        {
            case 0:
                audio.PlayOneShot(Attack1Sfx, 2.0f);
                break;
            case 1:
                audio.PlayOneShot(Attack2Sfx, 2.0f);
                break;
        }
    }
}
