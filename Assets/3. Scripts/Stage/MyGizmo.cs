using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGizmo : MonoBehaviour
{

   
    public enum Type {  NORMAL, ENEMYSPAWN}
    private const string wayPointFile = "Enemy";
   
    
    [Header("Basic Info")]
    public Type type = Type.NORMAL;
    public Color _color = Color.yellow;
    public float _radius = 0.1f;

    private void OnDrawGizmos()
    {
        if (type == Type.NORMAL)
        {
            Gizmos.color = _color;
            Gizmos.DrawSphere(transform.position, _radius);
        }
        else
        {
            //기즈모 색상 설정
            Gizmos.color = _color;
            //Enemy 이미지 표시
            Gizmos.DrawIcon(transform.position + Vector3.up * 1.0f, wayPointFile, true);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
