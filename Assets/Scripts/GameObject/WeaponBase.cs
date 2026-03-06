using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [Header("常规配置")]
    public int camp;
    public bool isTurn = true;
    public bool isDead;
    public float weaponTurnSpeed;
    

    [Header ("攻击配置")]
    public float damage;
    public float damageCoff;
    public float attackRange;    
    public float SightRange;
    public float attackCD;
    protected float currentAttackCD; // 当前攻击冷却（protected，子类更新）
    public E_BulletType type;

    [Header ("组件引用")]
    public Transform shootPoint;
    //public Transform turretTransform;
    public SoldierStructureBase father;
    public SoldierStructureBase lockTarget;
    private BulletBase bullet;

    private readonly RaycastHit[] hitInfos = new RaycastHit[20];

    public SoldierStructureBase LockTarget
    {
        get { return lockTarget; }
        set { lockTarget = value; }
    }

    protected virtual void Awake()
    {
        isDead = father.isDead;
        camp = father.camp;
        transform.SetParent(father.transform);  
        SightRange = attackRange + 0.8f;
        currentAttackCD = 0;
        if(shootPoint == null) shootPoint = gameObject.transform.Find("ShootPoint");
        // 初始化球形!碰撞体（触发器，适配视野范围）
        SphereCollider triggerCollider = GetComponent<SphereCollider> ();
        if(triggerCollider == null || !triggerCollider.isTrigger)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider> ();
        }
        triggerCollider.isTrigger = true; // 设置为触发器
        triggerCollider.radius = attackRange;
        
    }

    public List<SoldierStructureBase> enemiesInSightRange = new List<SoldierStructureBase> (); private Coroutine currentCheckCoroutine;
    public List<SoldierStructureBase> enemiesInAttackRange = new List<SoldierStructureBase>();

    public void OnTriggerEnter(Collider other)
    {
        if(father.isDead || other.isTrigger || other is MeshCollider) return;

        SoldierStructureBase enemy = other.GetComponentInParent<SoldierStructureBase> ();

        if(enemy == this || enemy == null || enemy.camp == this.camp || enemy.isDead) return;

        if(!enemiesInSightRange.Contains (enemy)) enemiesInSightRange.Add (enemy);

        if(enemiesInSightRange.Count > 0 && currentCheckCoroutine == null)
        {
            currentCheckCoroutine = StartCoroutine (IsEnemiesInList ());
        }
    }
    private IEnumerator IsEnemiesInList()//相当于update
    {
        bool isHit;int hits;
        Vector3 enemyDir;
        //RaycastHit hit;

        SoldierStructureBase enemy;

        //lockTarget = SerchEnemiesForDistance ();

        while(enemiesInSightRange.Count > 0 && father.isDead == false)
        {
            for(int i = enemiesInSightRange.Count - 1; i >= 0; i--)//筛选列表
            {
//#if UNITY_EDITOR
//                Physics.SyncTransforms (); // 强制同步Transform和物理引擎
//#endif
                enemy = enemiesInSightRange[i];
                enemyDir = (enemy.transform.position - transform.position).normalized;
                //Debug.DrawRay (transform.position, enemyDir * attackRange, Color.red);
                isHit = false;
                hits = Physics.RaycastNonAlloc (transform.position, enemyDir, hitInfos, SightRange, ~camp, QueryTriggerInteraction.Ignore);
                for(int j = 0; j < hits; j++)
                {
                    if(hitInfos[j].collider == enemy.realCollider) isHit = true;
                }
                //foreach(RaycastHit _hit in hitInfo)
                //{
                //    if(_hit.collider == enemy.realCollider) isHit = true;
                //}
                if(enemy == null || enemy.isDead || !isHit)
                {
                    enemiesInSightRange.Remove (enemy);
                }
            }
            if(enemiesInSightRange.Count == 0) break;

            SetLockTarget ();
            //找敌人逻辑
            //if(lockTarget == null || lockTarget.isDead || !enemiesInSightRange.Contains (lockTarget))
            //{
            //    lockTarget = SerchEnemiesForDistance ();
            //}
            //if(lockTarget == null || enemiesInSightRange.Count == 0) break;
            //\isHit = false;
            //if(currentAttackCD >= -1) currentAttackCD -= Time.deltaTime;
            //Physics.Raycast (transform.position, (lockTarget.transform.position - transform.position).normalized, out hit, SightRange, ~camp);
            ////Debug.DrawRay (transform.position, (lockTarget.transform.position - transform.position).normalized * attackRange, Color.red);
            //if(isTurn)
            //{
            //    if( RotateTurretToTarget () && hit.collider.gameObject.CompareTag ("Attackable") && currentAttackCD <= 0) PerformAttack ();
            //}
            //else if(hit.collider.gameObject.CompareTag ("Attackable") && isAim () && currentAttackCD <= 0) PerformAttack ();

            yield return null;
        }
        currentAttackCD = attackCD;
        currentCheckCoroutine = null;
        lockTarget = null;
    }

    private bool isAim()
    {
        Vector3 targetDir = lockTarget.transform.position - transform.position;
        Quaternion targetRot = Quaternion.LookRotation (targetDir);//四元坐标
        float angleDiff = Quaternion.Angle (transform.rotation, targetRot);
        return angleDiff < 6f;
    }

    public void SetLockTarget()
    {
        int hits;
        Vector3 enemyDir;
        SoldierStructureBase enemy;
        enemiesInAttackRange.Clear ();
        for(int i= enemiesInSightRange.Count-1;i>=0;i--)
        {
            enemy = enemiesInSightRange[i];
            enemyDir = (enemy.transform.position - transform.position).normalized;
            hits = Physics.RaycastNonAlloc (transform.position, enemyDir,hitInfos,attackRange,~camp, QueryTriggerInteraction.Ignore);
            for(int j = 0; j < hits; j++)//+enemy
            {
                if(hitInfos[j].transform.gameObject.layer == 3) break;
                if(hitInfos[j].collider == enemy.realCollider) 
                    enemiesInAttackRange.Add(enemy);
                
            }
        }
        
        if(lockTarget == null || lockTarget.isDead || !enemiesInAttackRange.Contains (lockTarget))
        {
            lockTarget = SerchEnemiesForDistance ();
        }
        if(lockTarget == null || enemiesInAttackRange.Count == 0) return;
        //isHit = false;
        if(currentAttackCD >= -1) currentAttackCD -= Time.deltaTime;
        //Physics.Raycast (transform.position, (lockTarget.transform.position - transform.position).normalized, out hit, SightRange, ~camp);
        //Debug.DrawRay (transform.position, (lockTarget.transform.position - transform.position).normalized * attackRange, Color.red);
        if(isTurn)
        {
            if(RotateTurretToTarget () && currentAttackCD <= 0) PerformAttack ();
        }
        else if(isAim () && currentAttackCD <= 0) PerformAttack ();
    }

    public bool RotateTurretToTarget()
    {
        Vector3 targetDir = lockTarget.transform.position - transform.position;
        Quaternion targetRot = Quaternion.LookRotation (targetDir);//四元坐标

        transform.rotation = Quaternion.RotateTowards (
        transform.rotation,
        targetRot,
        weaponTurnSpeed * Time.deltaTime);
        float angleDiff = Quaternion.Angle (transform.rotation ,targetRot);
        return angleDiff < 3f;
    }

    public SoldierStructureBase SerchEnemiesForDistance()//每转换一次目标执行一次 !!!保证列表中的敌人都是活得
    {
        if(enemiesInAttackRange.Count == 0) return null;
        float minDistance = 3000;
        SoldierStructureBase enemy = null;
        for(int i = 0; i < enemiesInAttackRange.Count; i++)
        {
            if(enemiesInAttackRange[i] == null || enemiesInAttackRange[i].isDead) continue;
            float distanceSqr = (transform.position - enemiesInAttackRange[i].transform.position).sqrMagnitude;
            if(minDistance > distanceSqr)
            {

                minDistance = distanceSqr;
                enemy = enemiesInAttackRange[i];
            }
        }
        return enemy;
    }

    public virtual void PerformAttack()
    {
        PoolMgr.Instance.GetObj ("Test_BulletTest", callback =>
        {
            callback.transform.position = shootPoint.position;
            callback.transform.rotation = shootPoint.rotation;
            bullet = callback.GetComponent<BulletBase> ();
            bullet.camp = camp;
            bullet.lockTarget = lockTarget;
            bullet.bulletType = type;
            bullet.damage = damage;
            bullet.damageCoff = damageCoff;
            StartCoroutine(bullet.MoveToTarget ());
        });
        currentAttackCD = attackCD;
    }
    
}
