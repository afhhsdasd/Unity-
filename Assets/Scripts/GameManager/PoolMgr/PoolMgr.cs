using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEditor;
//专门的数据结构，在Unity面板中模拟数据的存放状态
//入口
public class PoolMgr: Singleton<PoolMgr>
{
    public class PoolStructure
    {
        public GameObject pool;

        public Dictionary<string, GameObject> poolTag = new Dictionary<string, GameObject>();

        public PoolStructure()
        {
        
        }
        public void ReleasePool(GameObject obj)
        {
            //激活对象面板，显示激活对象就行,后续扩展写在这里

            obj.transform.parent = null;
        }

        //public List<string> poolTag = new List<string>();
        public void BuildPool(GameObject obj)
        {
            //失活对象面板,我得传参，把失活后的对象分门别类,如何装poolTag
            if(pool == null)
            {
                pool = new GameObject ("Pool");
            }//传进来obj，我先判断他是否有对应的Tag，没有就见一个

            if(!poolTag.ContainsKey (obj.name))
            {
                GameObject tag = new GameObject (obj.name);
                poolTag.Add (obj.name, tag);
                tag.transform.parent = pool.transform;
            }
        
            obj.transform.parent = poolTag[obj.name].transform;//差不多了就这样
        }
    }

    public PoolStructure poolStructure = new PoolStructure();
    //Pool:
    public Dictionary<string,Queue<GameObject>> poolDic = new Dictionary<string, Queue<GameObject>>();
    
    public void GetObj(string name,UnityAction<GameObject> callback)
    {
        GameObject obj = null;
        // 同步逻辑
        if(poolDic.ContainsKey(name) && poolDic[name].Count > 0)
        {

            obj = poolDic[name].Dequeue();

            obj.SetActive (true);
            poolStructure.ReleasePool (obj);
            obj.transform.parent = null;
            callback (obj);
        }
        //异步逻辑
        else
        {
            //ResMgr.Instance.LoadAsync<GameObject> (name, (objects) =>
            //{
            //    obj = objects;
            //    obj.name = name;
            //    callback(obj);
            //}); 
            AddressableMgr.Instance.LoadAssetAsync<GameObject> (modlecallback =>
            {
                obj = Instantiate(modlecallback);
                obj.name = name;
                callback (obj);//直接能用
            },name);
        }
        
    }
    
    public void PushObj(string name,GameObject obj)
    {
        
        poolStructure.BuildPool (obj);

        obj.SetActive (false);

        if(poolDic.ContainsKey(name))
        {
            poolDic[name].Enqueue(obj);
        }
        else
        {
            poolDic.Add(name, new Queue<GameObject> ()); poolDic[name].Enqueue (obj);
        }


    }

    public void Clear()
    {
        poolDic.Clear ();
        //poolView = null;
        poolStructure.poolTag.Clear ();
        poolStructure.pool = null;
    }
}
