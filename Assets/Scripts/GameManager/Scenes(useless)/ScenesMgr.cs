using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScenesMgr : Singleton<ScenesMgr>
{
    public void LoadScene(string sceneName, UnityAction func)
    {
        SceneManager.LoadScene (sceneName);
        func();
    }
    public void LoadSceneAsync(string sceneName,UnityAction func)
    {
        StartCoroutine (RealLoadSceneAsync(sceneName,func));
    }

    private IEnumerator RealLoadSceneAsync(string name,UnityAction func)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(name);
        while (!async.isDone)
        {
            EventCenter.Instance.EventTrigger("场景加载进度条更新",async.progress);
            yield return null;//执行到此暂停到每帧update执行完毕，同时设定下一次执行时机
        }
        func();
    }
}
