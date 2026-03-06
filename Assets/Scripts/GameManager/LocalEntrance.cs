using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//关于什么场景应该挂载什么样的管理脚本，顺便管理场景切换事务，通过代码控制
public class LocalEntrance : SingletonLocal<LocalEntrance>
{

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

}
