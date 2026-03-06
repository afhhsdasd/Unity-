using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResMgr : Singleton<ResMgr>
{
    public T Load<T>(string path) where T : Object
    {
        T obj = Resources.Load<T>(path);//可优化点
        if(obj is GameObject)
        {
            return GameObject.Instantiate(obj);
        }
        else
        {
            return obj;
        }
    }
    public void LoadAsync<T>(string path,UnityAction<T> callBack) where T : Object
    {
        StartCoroutine (RealLoadAsync<T> (path, callBack));
        //可以写扩展传调参数，从原来的方法集合变成单个方法的封装
    }
    private IEnumerator RealLoadAsync<T>(string path, UnityAction<T> callBack) where T : Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;//已懂,相当于while,没执行完之前不往下执行
        //假如循环100帧，协程会在这里卡100帧，协程正常运行，后面的代码卡着不动
        //资源加载完毕后执行回调
        if(request.asset is GameObject)
            callBack (GameObject.Instantiate(request.asset) as T);
        else
        {
            callBack (request.asset as T);
        }
    }
}
