using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //내비게이션 기능을 이용하기 위한 네임스페이스


[RequireComponent(typeof(NavMeshAgent))]
public class MoveAgent : MonoBehaviour
{
    [Header("Way Info")]
    public List<Transform> wayPoints; //순찰지점 저장 배열 변수
    public int nextIdx; //다음 순찰 지점 index

    private readonly float patrolSpeed = 1.5f;
    private readonly float traceSpeed = 4.0f;
    private float damping = 1.0f; //회전 시 속도 조절 계수


    private NavMeshAgent agent;
    private Transform enemyTr; //적 캐릭터 Transform 컴포넌트 저장

    private bool _patrolling; //순찰 여부 판단
    public bool patrolling //patrolling 프로퍼티 정의(getter, setter)
    {
        get { return _patrolling; }
        set
        {
            _patrolling = value;
            if(_patrolling)
            {
                agent.speed = patrolSpeed;
                damping = 1.0f; //순찰 상태에서의 회전계수
                MoveWayPoint();
            }
        }
    }

    private Vector3 _traceTarget; //추적 대상의 위치를 저장하는 변수
    public Vector3 traceTarget //traceTarget 프로퍼티 정의
    {
        get { return _traceTarget; }
        set
        {
            _traceTarget = value;
            agent.speed = traceSpeed;
            damping = 7.0f; //추적상태에서의 회전 계수
            TraceTarget(_traceTarget);
        }
    }

    //NavMeshAgent의 이동속도에 대한 프로퍼티 정의(getter)
    public float speed
    {
        get { return agent.velocity.magnitude; }
    }

    void Start()
    {
        enemyTr = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false; //목적지에 가까울수록 속도 줄이는 옵션 비활성화
        agent.updateRotation = false; //자동으로 회전하는 옵션 비활성화
        agent.speed = patrolSpeed;

        var group = GameObject.Find("WayPointGroup");
        if (group != null)
        {
            //WayPointGroup 하위에 있는 모든 Transfomr 컴포넌트를 추출한 후
            //List 타입의 wayPoints 배열에 추가
            group.GetComponentsInChildren<Transform>(wayPoints);
            wayPoints.RemoveAt(0); //배열 첫 항목 삭제

            nextIdx = Random.Range(0, wayPoints.Count);  //첫번째로 이동할 위치를 불규칙하게 추출
        }

        MoveWayPoint();
    }

    void MoveWayPoint()
    {
        if (agent.isPathStale) return; //최단거리 경로 계산이 끝나지 않으면 수행 X

        agent.destination = wayPoints[nextIdx].position;//다음 목적지를 wayPoints 배열에서 추출한 위치로 다음 목적지를 지정
        agent.isStopped = false; //내비게이션 기능을 활성화 해서 이동 시작
    }
    
    void TraceTarget(Vector3 pos) //주인공 추적시 이동 함수
    {
        if (agent.isPathStale) return;

        agent.destination = pos;
        agent.isStopped = false;
    }

    public void Stop() //순찰 및 추적을 정지시키는 함수
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; //바로 정지하기 위해 속도 0으로 설정
        _patrolling = false;
    }

    void Update()
    {

        if(agent.isStopped == false)
        {
            Quaternion rot = Quaternion.LookRotation(agent.desiredVelocity); //NavMeshAgent 가  가야 할 방향 벡터를 쿼터니언 타입의 각도로 변환
            enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, rot, Time.deltaTime * damping); //보간 함수를 사용해 점진적으로 회전시킴
        }
        if (!_patrolling) return; //순찰 모드가 아닌 경우 이후 로직 수행 X

        if(agent.velocity.sqrMagnitude >= 0.2f*0.2f && agent.remainingDistance <= 0.5f)
        { //NavMeshAgent가 이동하고 있고 목적지에 도착했는지 계산

            //nextIdx = ++nextIdx % wayPoints.Count; //다음 목적지의 배열 첨자 계산
            nextIdx = Random.Range(0, wayPoints.Count); 

            MoveWayPoint(); //다음 목적지로 이동 명령
        }
    }
}
