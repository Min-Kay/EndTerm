using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Damage : MonoBehaviour
{

    private const string bulletTag = "E_Bullet"; 
    private const string SpikeTag = "Spike";
    private Shake shake;

    [Header("HP Info")]
    public Text hpBar;
    public float initHP = 100.0f;
    public float currHP;

    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;

    void Start()
    {
        currHP = initHP;
        CheckHp();
        shake = GetComponent<Shake>();
    }

    private void OnTriggerEnter(Collider coll) 
    {
        if (coll.tag == bulletTag) 
        {
            Destroy(coll.gameObject);

            currHP -= 5.0f;

            CheckHp();
            StartCoroutine(shake.ShakeCamera(0.05f,0.1f,0.15f));
        }
        else if(coll.tag == SpikeTag && GameManager.instance.isBossAttack == true)
        {

            currHP -= 15.0f;

            CheckHp();
            StartCoroutine(shake.ShakeCamera(0.05f,0.2f,0.2f));

        }

        if (currHP <= 0.0f) 
        {
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        CheckHp();
        OnPlayerDie();
        GameManager.instance.isGameOver = true;
        GameManager.instance.IncKillCount();
    }

    public void CheckHp()
    {
        if (currHP >= 50)
        {
            hpBar.text = "HP  " + currHP.ToString();
            hpBar.color = new Vector4(0, 1, 0, 1);
        }
        else if (currHP >= 30 && currHP <50)
        {
            hpBar.text = "HP  " + currHP.ToString();
            hpBar.color = new Vector4(1, 1, 0, 1);
        }
        else if(currHP < 30)
        {
            if (currHP < 0)
                currHP = 0;

            hpBar.text = "HP  " + currHP.ToString();
            hpBar.color = new Vector4(1, 0, 0, 1);
        }
    }

    IEnumerator bossHit()
    {
        yield return new WaitForSeconds(1.0f);
    }
}
