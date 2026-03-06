using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum E_SoldierState
{
    Idle,     // 闲置
    Moving,   // 移动中
    Attacking,// 攻击中
    //Dead,     // 死亡
    //Hurt      // 受伤硬直（可选，小项目可省略）
}

public class ArmorBody : SoldierStructureBase
{
    //炮台塔座，自身旋转，只根据玩家移动方向进行旋转
    [Header ("常规配置")]
    public float moveSpeed;
    public float bodyTurnSpeed;
    //E_SoldierState state;

    [Header ("寻路配置")]
    public bool isAutoFindWay = false; // 是否开启自动寻路（我方可选，敌方默认true，）
    //protected Vector3 moveTargetPos;

    [Header ("组件引用")]
    public Transform weapontPoint;
    public WeaponBase mainWeapon;

    public Animation bodyAnimation;
    public NavMeshAgent bodyAgent;
    private GameObject footEffect;
    
    private Coroutine coroutineT;
    private Coroutine cortMainWeapon;
    //private SoldierStructureBase target = null;

    //public SoldierStructureBase Target
    //{
    //    get { return target; }
    //}



    public GameObject FootEffect
    {
        get
        {
            return footEffect;
        }
        
    }

    //public List<Coroutine> coroutines = new List<Coroutine>();

    protected override void Awake()
    {
        base.Awake();
        bodyAnimation = GetComponentInChildren<Animation>();
        bodyAgent = GetComponentInChildren<NavMeshAgent> ();
        footEffect = transform.Find ("FootEffect").gameObject;footEffect.SetActive (false);
    }

    public IEnumerator TurrentReturn(WeaponBase weapon)
    {
        float angle = Quaternion.Angle (transform.rotation, weapon.transform.rotation);

        while(angle > 0.5f)
        {
            if(mainWeapon.enemiesInAttackRange.Count != 0) yield break;
            yield return null;

            weapon.transform.rotation = Quaternion.RotateTowards (
                weapon.transform.rotation,
                transform.rotation,
                weapon.weaponTurnSpeed * Time.deltaTime);
        }
        
        weapon.transform.rotation = transform.rotation;
    }

    public virtual void ArmorMove(Vector3 moveTargetPos)
    {
        //玩家移动
        //bodyAgent.Move (moveTargetPos);瞬移
        bodyAgent.SetDestination (moveTargetPos);
        if(cortMainWeapon != null)
        {
            StopCoroutine (cortMainWeapon);
            cortMainWeapon = null;
        }
        cortMainWeapon = StartCoroutine (TurrentReturn(mainWeapon));

        StartCoroutine(TurrentReturn (mainWeapon));
        
        if(coroutineT != null)
        {
            StopCoroutine(coroutineT);
            coroutineT = null;
        }
    }

    //public virtual void AutoFindPath()
    //{

    //}

    public void SetTarget(SoldierStructureBase target)
    {
        if(coroutineT != null)
        {
            StopCoroutine (coroutineT);
            coroutineT = null;
        }

        coroutineT = StartCoroutine (MoveToTarget(target));

    }

    private IEnumerator MoveToTarget(SoldierStructureBase target)
    {
        while(!target.isDead)
        {
            yield return null;
            
            //int waitFrames = 5;
            //for(int i = 0; i < waitFrames; i++)
            //{
            //    // 每循环一次等待1帧，同时检测目标是否死亡
            //    if(target.isDead) yield break;
            //    yield return null;
            //}

            if(mainWeapon.enemiesInAttackRange.Contains (target) && mainWeapon.LockTarget != target)
            {
                mainWeapon.LockTarget = target;
                bodyAgent.ResetPath ();
                continue;
            }

            bodyAgent.SetDestination (target.transform.position);

        }
        coroutineT = null;
    }
}
