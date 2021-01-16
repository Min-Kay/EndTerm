using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager: MonoBehaviour
{
    [Header("Enemy Create Info")]  //가독성을 높이기 위한 헤더 
    public Transform[] enemyPoints; //출현 위치 배열
    public Transform bossPoint;
    public GameObject enemy; //적 프리팹 저장 변수
    public GameObject boss;
    public float enemyCreateTime = 2.0f; //적 생성 주기
    public int maxEnemy = 10; //한번에 공존 가능한 최대 적 수
    private int maxCount = 0; //이번 게임에서 현재 총 생성된 적 수
    public int finRound = 10; //라운드 총 생성 적 수
   [HideInInspector] public float bossHp = 0;

    [Header("Item Create Info")]
    public Transform[] itemPoints; //아이템 출현 위치 배열
    public GameObject healBlock;
    public GameObject damageBlock;
    public GameObject speedBlock; 
    public float itemCreateTime = 10.0f; //아이템 생성 주기
    public int maxItem =4; //한번에 공존 가능한 최대 아이템 수


    [Header("Game Info")]
    public bool isGameOver = false; //패배 여부
    public bool isClear = false; //승리 여부
    public bool bossSpawn = false; //보스 출현 여부
    public bool gameSet = false; //게임 종료 여부 

    [Header("UI Info")]
    [HideInInspector] public int killCount; //처치한 적 수
    public Text killCountTxt; //처치수 출력 UI 
    public Text bossHpTxt;
    public Text mainTxt;
    public Text noticeTxt;

    [HideInInspector] public bool isBossAttack = false; //보스의 공격 여부에 따른 콜라이더 충돌 판정

    //아이템의 사용에 따른 변화 텍스트화
    [Header("Item Check Info")]
    public bool isHeal = false;
    public bool isDamage = false;
    public bool isSpeed = false;

    public bool isFullHp = false;
    public bool isFullDm = false;
    public bool isFullSp = false;

    public float damageAmount = 0;
     public float speedAmount = 0;

    public float addHeal = 0;
    public float addDamage = 0;
    public float addSpeed = 0;

    [Header("Win Lose SFX Info")]
    private AudioSource audio;
    public AudioClip winSfx;
    public AudioClip loseSfx;

    private Vector4 Red = new Vector4(1.0f, 0.0f, 0.0f, 1);
    private Vector4 Green = new Vector4(0.0f, 1.0f, 0.0f, 1);
    private Vector4 Yellow = new Vector4(1.0f, 1.0f, 0.0f, 1);

    public static GameManager instance = null; //싱글턴에 접근하기 위한 Static 변수 선언


    void Awake() //instance 판단 여부를 위해 가장 먼저 실행되는 Awake 선언 
    { 
            if(instance == null)
        {
            instance = this; //null일 경우 GameManager을 할당
        }
            else if (instance != this) //instance에 할당된 클래스의 인스턴스가 다를 경우 새로 생선된 클래스를 의미
        {
            Destroy(this.gameObject);
        }

        LoadGameData();
        DontDestroyOnLoad(this.gameObject); //다른 씬으로 넘어가도 삭제하지 않고 유지
        //게임 초기 데이터 로드

        audio = GetComponent<AudioSource>();
    }

    void LoadGameData()
    {
        killCount = finRound;
        killCountTxt.text = killCount.ToString() + "  Left";

    }

    void Start()
    {

        enemyPoints = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>(); //SpawnPointGroup 내의 Points들에 접근
        itemPoints = GameObject.Find("ItemPointGroup").GetComponentsInChildren<Transform>();
        bossPoint = GameObject.Find("BossSpawnPoint").GetComponent<Transform>();

        if (enemyPoints.Length > 0)
        {
            StartCoroutine(this.CreateEnemy());
        }

        if(itemPoints.Length > 0)
        {
            StartCoroutine(this.CreateItem());
        }

        StartCoroutine(this.noticeWrite());
        StartCoroutine(this.WinOrLose());
    }

    IEnumerator CreateEnemy()
    {
        while (!gameSet) //게임 종료까지 반복 
        {
            int enemyCount = (int)GameObject.FindGameObjectsWithTag("Enemy").Length; //현재 생성된 적의 수 

            if (enemyCount < maxEnemy && killCount > 0 && maxCount < finRound)
            {
                yield return new WaitForSeconds(enemyCreateTime); //생성 주기 시간만큼 대기

                int idx = Random.Range(1, enemyPoints.Length); //불규칙 위치 산출 

                Instantiate(enemy, enemyPoints[idx].position, enemyPoints[idx].rotation); //적 동적 생성
                maxCount++;
            }
            else if (killCount == 0 && bossSpawn == false)
            {
                yield return new WaitForSeconds(enemyCreateTime);
                Instantiate(boss,bossPoint.position, bossPoint.rotation);
                bossSpawn = true;
            }
            else
                yield return null;
        }
    }

    IEnumerator CreateItem()
    {
        int itemCount = ((int)GameObject.FindGameObjectsWithTag("HealBlock").Length + (int)GameObject.FindGameObjectsWithTag("DamageBlock").Length + (int)GameObject.FindGameObjectsWithTag("SpeedBlock").Length);

        while (!gameSet)
        {
            if (itemCount < maxItem)
            {
                int idx = Random.Range(1, itemPoints.Length); //불규칙 위치 산출 

                yield return new WaitForSeconds(itemCreateTime); //생성 주기 시간만큼 대기

                Instantiate(chooseBlock(Random.Range(0, 3)), itemPoints[idx].position, itemPoints[idx].rotation) ; //적 동적 생성
            }
            else
                yield return null;
        }
    }

    IEnumerator noticeWrite()
    {
        while (!gameSet)
        {
            if (isHeal == true)
            {
                noticeTxt.color = Green;
                noticeTxt.text = "+" + addHeal.ToString() + " Heal";
                yield return new WaitForSeconds(1.5f);
                noticeTxt.text = " ";
                isHeal = false;
            }
            else if (isSpeed == true)
            {
                noticeTxt.color = Green;
                noticeTxt.text = "+" + addSpeed.ToString() + "  | Speed : " + speedAmount.ToString();
                yield return new WaitForSeconds(1.5f);
                noticeTxt.text = " ";
                isSpeed = false;
            }
            else if (isDamage == true)
            {
                noticeTxt.color = Green;
                noticeTxt.text = "+" + addDamage.ToString() + "  | Damage : " + damageAmount.ToString();
                yield return new WaitForSeconds(1.5f);
                noticeTxt.text = " ";
                isDamage = false;
            }
            else if (isFullHp == true)
            {
                noticeTxt.color = Yellow;
                noticeTxt.text = "Full Hp";
                yield return new WaitForSeconds(1.5f);
                noticeTxt.text = " ";
                isFullHp = false;
            }
            else if (isFullDm == true)
            {
                noticeTxt.color = Yellow;
                noticeTxt.text = "Full Damage : " + damageAmount.ToString();
                yield return new WaitForSeconds(1.5f);
                noticeTxt.text = " ";
                isFullDm = false;
            }
            else if (isFullSp == true)
            {
                noticeTxt.color = Yellow;
                noticeTxt.text = "Full Acclaration : " + speedAmount.ToString();
                yield return new WaitForSeconds(1.5f);
                noticeTxt.text = " ";
                isFullSp = false;
            }
            else
                yield return null;
        }
    }
 
    IEnumerator WinOrLose()
    {
        while (!gameSet)
        {
            if (isClear)
            {
                gameSet = true;
                yield return new WaitForSeconds(0.2f);
                audio.PlayOneShot(winSfx);
            }
            else if (isGameOver)
            {
                gameSet = true;
                yield return new WaitForSeconds(0.2f);
                audio.PlayOneShot(loseSfx);
            }
            else
                yield return null;
        }
    }

    public void IncKillCount()
    {
        if (isGameOver)
        {
            killCountTxt.text = "GAME OVER";
            mainTxt.text = "YOU DIE";
            mainTxt.color = Red;
        }
        else if (isClear)
        {
            killCountTxt.text = "CLEAR";
            mainTxt.text = "YOU WIN";
            mainTxt.color = Green;
        }
        else if (killCount > 0)
        {
            killCount--;
           
            if(killCount > 0)
                killCountTxt.text = killCount.ToString() + "  Left";
            else if (killCount == 0)
                killCountTxt.text = "BOSS STAGE";
        }
    }

    public void IncBossHp()
    {   
        if (bossHp > 0)
            bossHpTxt.text = "Boss HP :  " + bossHp.ToString();
        else if (bossHp <= 0)
        {
            bossHp = 0;
            bossHpTxt.text = " ";
        }
    }

    public GameObject chooseBlock(int i)
    {
        switch (i) {
            case 0:
                return healBlock;
                break;
            case 1:
                return speedBlock;
                break;
            case 2:
                return damageBlock;
                break;
        }

        return null;

    }

}
