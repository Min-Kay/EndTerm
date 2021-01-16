using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyAI : MonoBehaviour
{
    public enum State //적 상태 표현 열거형 변수
    {
        PATROL,
        TRACE,
        CHASE,
        ATTACK,
        DIE
    }

    [Header("Current State Info")]
    public State state = State.PATROL; //상태 저장
    public bool isDie = false; //사망여부 판단 
    public bool isPlayed = false;

    private Transform playerTr; //플레이어 위치 저장
    private Transform enemyTr; //적 위치 저장 
    private Animator animator; // Animator 컴포넌트 저장 변수

    [Header("Following Distance Info")]
    public float attackDist = 10.0f; //공격 사정거리
    public float traceDist = 12.0f; //추적 사정거리
    
    private WaitForSeconds ws; //코루틴에서 사용할 지연시간 변수
    private MoveAgent moveAgent; //이동을 제어하는 MoveAgent 클래스 저장 변수
    private EnemyFire enemyFire; //총알 발사 제어
    private AudioSource audio;

    [Header("SFX Info")]
    public AudioClip findSfx;
    public AudioClip dieSfx;

    //애니메이터 컨트롤러에 정의한 파라미터의 해시값을 미리 추출
    private readonly int hashMove = Animator.StringToHash("IsMove"); 
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashDieIdx = Animator.StringToHash("DieIdx");
    private readonly int hashOffset = Animator.StringToHash("Offset");
    private readonly int hashWalkSpeed = Animator.StringToHash("WalkSpeed");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");

    void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("PLAYER"); //주인공 게임오브젝트 추출
        if (player != null)
            playerTr = player.GetComponent<Transform>(); //주인공 Transform 추출


        enemyTr = GetComponent<Transform>();// 적 Transform 추출
        animator = GetComponent<Animator>();
        moveAgent = GetComponent<MoveAgent>();
        enemyFire = GetComponent<EnemyFire>();
        audio = GetComponent<AudioSource>();

        ws = new WaitForSeconds(0.3f); //코루틴 지연시간 생성

        //AI별로 다른 움직임을 가지기 위한 불규칙성 부여
        animator.SetFloat(hashOffset, UnityEngine.Random.Range(0.0f, 1.0f)); //Cycle offset 값을 불규칙하게 변경
        animator.SetFloat(hashWalkSpeed, UnityEngine.Random.Range(1.0f, 1.2f)); //Speed값을 불규칙하게 변경
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
        while(!isDie)
        {
            if(state == State.DIE) yield break; //사망시 코루틴 종료

            //주인공과 적 간의 거리 계산
            float dist = Vector3.Distance(playerTr.position, enemyTr.position);

            //공격 사정거리이내인 경우
            if(dist<=attackDist )
            {
                state = State.ATTACK;
            }
            else if (dist <= traceDist) //추적 사정거리이내인 경우
            {
                state = State.TRACE;
            }
            else if(GetComponent<EnemyDamage>().getShot == true)
            {
                state = State.CHASE;
            }
            else
            {
                state = State.PATROL;
            }

            yield return ws; //0.3초 동안 대기하는 동안 제어권 양보
        }
    }

    IEnumerator Action() //상태에 따라 행동처리하는 코루틴
    {
        while(!isDie) //적이 사망하기 전까지 무한루프
        {
            yield return ws;

            switch (state) //상태에 따른 분기
            {
                case State.PATROL:
                    enemyFire.isFire = false;
                    moveAgent.patrolling = true;
                    animator.SetBool(hashMove, true);
                    break;
                case State.TRACE:
                    Find();
                    enemyFire.isFire = false;
                    moveAgent.traceTarget = playerTr.position;
                    animator.SetBool(hashMove, true);
                    break;
                case State.ATTACK:
                    moveAgent.Stop();
                    animator.SetBool(hashMove, false);
                    GetComponent<EnemyDamage>().getShot = false;
                    if (enemyFire.isFire == false) //총알 발사 시작 
                        enemyFire.isFire = true;
                    break;
                case State.CHASE:
                    Find();
                    moveAgent.traceTarget = playerTr.position;
                    animator.SetBool(hashMove, true);
                    break;
                case State.DIE:
                    isDie = true;
                    enemyFire.isFire = false;
                    moveAgent.Stop();
                    audio.PlayOneShot(dieSfx);
                    animator.SetInteger(hashDieIdx, UnityEngine.Random.Range(0, 3)); //사망 애니메이션 종류 지정
                    animator.SetTrigger(hashDie); //사망 애니메이션 실행
                    GetComponent<CapsuleCollider>().enabled = false; //충돌 판정 비활성화
                    break;
            }
        }
    }

   void Update()
    {
        animator.SetFloat(hashSpeed, moveAgent.speed); //Speed 파라미터에 이동속도 전달  
    }

    public void OnPlayerDie() //플레이어 사망시 호출됨. Damage 함수 참고 
    {
        moveAgent.Stop();
        enemyFire.isFire = false;
        StopAllCoroutines(); //모든 코루틴 함수 종료

        animator.SetTrigger(hashPlayerDie);
    }

    public void Find()
    {
        if (isPlayed == false)
        {
            audio.PlayOneShot(findSfx);
            isPlayed = true;
        }
    }
}
