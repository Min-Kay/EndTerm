using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyFire : MonoBehaviour
{

    private AudioSource audio;
    private Animator animator;
    private Transform playerTr;
    private Transform enemyTr;

    private readonly int hashFire = Animator.StringToHash("Fire"); //애니메이터 컨트롤러에 정의한 파라미터의 해시값을 미리 추출
    private readonly int hashReload = Animator.StringToHash("Reload");

    private float nextFire = 0.0f; //다음 발사할 시간 계산용 변수
    private readonly float fireRate = 0.4f; // 총알 발사 간격
    private readonly float damping = 10.0f; // 주인공을 향해 회전할 속도 계수

    private readonly float reloadTime = 3.0f; //재장전 시간
    private readonly int maxBullet = 10; //탄창의 최대 총알 수
    private int currBullet = 10; //초기총알 수
    private bool isReload = false; //재장전 여부
    private WaitForSeconds wsReload; //재장전 시간동안 기다릴 변수

    [Header("Fire Info")]
    public GameObject Bullet;
    public Transform firePos;

    [Header("Check Info")]
    public bool isFire = false; //총알 발사 여부를 판단할 변수
    
    [Header("SFX Info")]
    public AudioClip fireSfx;
    public AudioClip reloadSfx;

    void Start()
    {
        //컴포넌트 추출 및 변수 저장
        playerTr = GameObject.FindGameObjectWithTag("PLAYER").GetComponent<Transform>();
        enemyTr = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
       
        wsReload = new WaitForSeconds(reloadTime);
      
    }

    void Update()
    {
        if(!isReload && isFire)
        {
            if(Time.time >= nextFire) //현재 시간이 다음 발사 시간보다 큰지를 확인
            {
                Fire();
                nextFire = Time.time + fireRate + Random.Range(0.0f, 0.3f); //다음 발사 시간 계산
            }

            Quaternion rot = Quaternion.LookRotation(playerTr.position - enemyTr.position); //주인공이 있는 위치까지의 회전각도 계산
            enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, rot, Time.deltaTime * damping); //보간함수를 사용해 점진적으로 회전시킴 

        }
    }

    void Fire()
    {
        animator.SetTrigger(hashFire);
        audio.PlayOneShot(fireSfx, 1.0f);
        //StartCoroutine(ShowMuzzleFlash()); //그냥 Muzzle을 실행해도 보이나 깜빡임이 너무 빨라 천천히 보여주기위해 코루틴을 사용해 기다렸다가 꺼진다 

        GameObject _bullet = Instantiate(Bullet, firePos.position, firePos.rotation); //총알 생성
        Destroy(_bullet, 1.0f); //일정시간 지난후 삭제

        isReload = (--currBullet % maxBullet == 0);
        if(isReload)
        {
            StartCoroutine(Reloading());
        }
    }

    IEnumerator Reloading()
    {
        animator.SetTrigger(hashReload); //재장전 애니메이션 실행
        audio.PlayOneShot(reloadSfx, 1.0f); // 재장전 사운드 발생

        yield return wsReload; //재장전 시간만큼 대기하는 동안 제어권 양보

        currBullet = maxBullet; //총알 갯수 초기화
        isReload = false;
    }

   
}
