using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public enum E_BulletType
{
    Nolmal,//轻甲伤害1.2 重甲伤害0.8
    Pieracing,//轻甲伤害0.8 重甲伤害1.2
    Special,//轻甲伤害1 重甲伤害1
}
public class BulletBase : MonoBehaviour
{
    
    [Header("基本配置")]
    public float speed;
    public float duration = 1;
    public int camp;
    public float damage;
    public float damageCoff;                  
    public E_BulletType bulletType;
    public SoldierStructureBase lockTarget;

    [Header ("组件引用")]
    public Rigidbody rb;
    public Collider triggleCollider;

    protected virtual void Awake()
    {//从生成者那里那目标数据
        
        rb = GetComponent<Rigidbody> ();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody> ();
            rb.isKinematic = true;
        }

        triggleCollider = GetComponent<SphereCollider> ();
        if(triggleCollider == null)
        {
            triggleCollider = gameObject.AddComponent<SphereCollider> ();
            triggleCollider.isTrigger = true;
        }
    }

    private void Start()
    {

    }
    public IEnumerator MoveToTarget()
    {
        float moveTimer = 0;
        float movestep;
        Vector3 rayOrigin;
        Vector3 moveDir = (lockTarget.transform.position - transform.position).normalized;
        gameObject.layer = (int)Mathf.Log (camp, 2);
        RaycastHit[] hitInfos = new RaycastHit[10]; 
        //初始特效，音效

        while(moveTimer < duration)
        {
            rayOrigin = transform.position;
            moveTimer += Time.deltaTime;    
            movestep = speed * Time.deltaTime;
            int hitcount = Physics.RaycastNonAlloc (rayOrigin,moveDir,hitInfos, movestep, ~camp, QueryTriggerInteraction.Ignore);

            transform.Translate (moveDir * speed * Time.deltaTime, Space.World);
            for(int i = 0; i < hitcount; i++)
            {
                if(hitInfos[i].collider.gameObject.CompareTag("Attackable"))
                {
                    lockTarget.GetInjured(damage * damageCoff, bulletType);
                    //敌人特效，音效
                    PoolMgr.Instance.PushObj (gameObject.name, gameObject);
                    yield break;
                }
                if(hitInfos[i].collider.gameObject.layer == 3)
                {
                    //场景特效，音效
                    PoolMgr.Instance.PushObj (gameObject.name, gameObject);
                    yield break;
                }
            }
            yield return null;
        }

        PoolMgr.Instance.PushObj (gameObject.name, gameObject);
    }
    

}
