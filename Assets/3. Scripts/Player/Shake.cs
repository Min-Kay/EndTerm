using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    public Transform shakeCamera; //셰이크 효과를 줄 카메라의 Transform 
    public bool shakeRotate = false; //카메라회전 판단 변수

    //초기 좌표와 회전값을 저장할 변수
    private Vector3 originPos;
    private Quaternion originRot;

    private bool bossborn = false;

    void Start()
    {
        //카메라의 초깃값을 저장
        originPos = shakeCamera.localPosition;
        originRot = shakeCamera.localRotation;
    }

    public IEnumerator ShakeCamera(float duration = 0.05f, float magnitudePos = 0.03f, float magnitudeRot = 0.1f)
    {
        float passTime = 0.0f; //지나간 시간을 누적할 변수

        while(passTime<duration) //진동시간 동안 루프를 순회함
        {
            Vector3 shakePos = Random.insideUnitSphere; //불규칙한 위치를 산출
            shakeCamera.localPosition = shakePos * magnitudePos; //카메라의 위치를 변경

            if (shakeRotate) //불규칙 회전 사용시
            {
                Vector3 shakeRot = new Vector3(0, 0, Mathf.PerlinNoise(Time.time * magnitudeRot, 0.0f)); //불규칙한 회전값을 펄린 노이즈 함수를 이용해 추출

                shakeCamera.localRotation = Quaternion.Euler(shakeRot); //카메라의 회전값을 변경
            }
            passTime += Time.deltaTime; //진동 시간을 누적

            yield return null;
        }

        //진동이 끝난후 초깃값으로 설정 
        shakeCamera.localPosition = originPos; 
        shakeCamera.localRotation = originRot;
    }

}
