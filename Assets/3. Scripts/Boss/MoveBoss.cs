using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //내비게이션 기능을 이용하기 위한 네임스페이스


[RequireComponent(typeof(NavMeshAgent))]
public class MoveBoss : MonoBehaviour
{
    private readonly float traceSpeed = 7.0f;
    private float damping = 7.0f; //회전 시 속도 조절 계수

    private NavMeshAgent agent;
    private Transform enemyTr; //적 캐릭터 Transform 컴포넌트 저장

    private Vector3 _traceTarget; //추적 대상의 위치를 저장하는 변수
    private Quaternion rot;
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
        agent.speed = traceSpeed;
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
    }

    void Update()
    {
        if (agent.isStopped == false)
        {
            rot = Quaternion.LookRotation(agent.desiredVelocity); //NavMeshAgent 가  가야 할 방향 벡터를 쿼터니언 타입의 각도로 변환
            enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, rot, Time.deltaTime * damping); //보간 함수를 사용해 점진적으로 회전시킴
        }
    }
}
