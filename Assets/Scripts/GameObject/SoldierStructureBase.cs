using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

//public enum E_ColliderType
//{
//    Capsule,
//    Box,
//    Mesh,
//    Sphere
//}


public class SoldierStructureBase:MonoBehaviour
{
    //最终伤害 = 基础伤害 × (1 - 护甲减免比例)
    //护甲减免比例 = 护甲值 ÷ (护甲值 + 100)
    [Header ("常规配置")]
    public float defence;//*是否要加入护甲类型？  
    //public E_ColliderType colliderType;
    public int camp;

    

    [Header ("生命值配置")]
    public float maxHP;          // 最大生命值
    protected float currentHP;   // 当前生命值（protected，子类可访问，外部通过属性获取）
    public bool isDead = false;  // 是否死亡（标记位，简化状态判断）
    public float CurrentHP
    {
        get { return currentHP; }
    }


    [Header ("组件引用")]
    protected Rigidbody rb;             // 刚体（移动/转向）
    public Collider realCollider;
    //public Transform turretTransform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody> ();
        currentHP = maxHP; // 初始化当前生命值
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody> ();
            rb.isKinematic = true; // 运动学刚体：不受物理引擎力的影响，仅通过代码控制位置
            rb.useGravity = true; 
        }

        realCollider = GetComponent<Collider>();
       
        camp = 1 << gameObject.layer;
    }

    public virtual void GetInjured(float damage, E_BulletType bulletType) //传入IsInAttackRange
    {

    }

    public virtual void Die()
    {
        isDead = true;PoolMgr.Instance.PushObj (gameObject.name, gameObject);
    
    }
    //private List<SoldierStructureBase> enemiesInAttackRange = new List<SoldierStructureBase> (); private Coroutine currentCheckCoroutine;

    //public void OnTriggerEnter(Collider other)
    //{
    //    if(isDead || other.isTrigger ||other is MeshCollider) return;

    //    SoldierStructureBase enemy = other.GetComponentInParent<SoldierStructureBase> ();

    //    if(enemy == this || enemy == null || enemy.camp == this.camp || enemy.isDead) return;

    //    if(!enemiesInAttackRange.Contains (enemy)) enemiesInAttackRange.Add (enemy);

    //    if(enemiesInAttackRange.Count > 0 && currentCheckCoroutine == null)
    //    {
    //        currentCheckCoroutine = StartCoroutine (IsEnemiesInList ());
    //    }
    //}

    //private IEnumerator IsEnemiesInList()//相当于update
    //{
    //    bool isHit;
    //    RaycastHit[] hits = null; 
    //    SoldierStructureBase enemy;
    //    Vector3 enemyDir;
    //    lockTarget = SerchEnemiesForDistance ();
    //    while(enemiesInAttackRange.Count > 0 && isDead == false)
    //    {
    //        for(int i = enemiesInAttackRange.Count - 1; i >=0;i--)
    //        {
    //            enemy = enemiesInAttackRange[i];
    //            enemyDir = (enemy.transform.position - transform.position).normalized;Debug.DrawRay (transform.position, enemyDir * attackRange, Color.red);
    //            isHit = false;
    //            hits = Physics.RaycastAll (transform.position, enemyDir, fakeAttackRange, ~camp, QueryTriggerInteraction.Ignore);
    //            foreach(RaycastHit hit in hits)
    //            {
    //                if(hit.transform == enemy.transform) isHit = true;
    //            }

    //            if(enemy == null || enemy.isDead || !isHit )
    //            {
    //                enemiesInAttackRange.Remove(enemy);
    //            }
    //        }

    //        if(enemiesInAttackRange.Count == 0) yield return null;

    //        if(currentAttackCD >= 0) currentAttackCD -= Time.deltaTime;
    //        RotateTurretToTarget ();

    //        yield return null;
    //    }
    //    currentCheckCoroutine = null;
    //    lockTarget = null;
    //}

    //public bool RotateTurretToTarget()
    //{
    //    if(lockTarget == null || lockTarget.isDead || !enemiesInAttackRange.Contains(lockTarget))
    //    {
    //        lockTarget = SerchEnemiesForDistance ();
    //    }
    //    if(lockTarget == null || enemiesInAttackRange.Count == 0) return false;
    //    Vector3 targetDir = lockTarget.transform.position - turretTransform.position;
    //    Quaternion targetRot = Quaternion.LookRotation (targetDir);//四元坐标

    //    turretTransform.rotation = Quaternion.RotateTowards (
    //    turretTransform.rotation,
    //    targetRot,
    //    turretTurnSpeed * Time.deltaTime);
    //    float angleDiff = Quaternion.Angle (turretTransform.rotation, targetRot);

    //    return angleDiff < 1f;
    //}

    //public SoldierStructureBase SerchEnemiesForDistance()//每转换一次目标执行一次 !!!保证列表中的敌人都是活得
    //{
    //    if(enemiesInAttackRange.Count == 0) return null;
    //    float minDistance = float.MaxValue;
    //    SoldierStructureBase enemy = null;
    //    for(int i = 0;i < enemiesInAttackRange.Count;i++) 
    //    {
    //        if(enemiesInAttackRange[i] == null || enemiesInAttackRange[i].isDead) continue;
    //        float distanceSqr = (transform.position - enemiesInAttackRange[i].transform.position).sqrMagnitude;
    //        if(minDistance > distanceSqr)
    //        {

    //            minDistance = distanceSqr;
    //            enemy = enemiesInAttackRange[i];
    //        }
    //    }
    //    return enemy;
    //}





}
