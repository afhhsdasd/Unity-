using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
//一、核心思路 1：分层抽象（通用层 + 资源类型专属层）
//保留你现有代码的「通用基础逻辑」（缓存管理、Handle 状态判断、通用回调），把不同资源类型的专属逻辑抽成 “分支层”，避免一锅烩：
//通用基础层：提取你代码中「缓存 key 生成、Handle 缓存、状态判断（IsDone/Status）、失败清理缓存」这些通用逻辑，封装成私有方法（比如CreateCacheKey()、CheckCachedHandle()），作为所有加载逻辑的底层；
//资源类型专属层：基于基础层，为不同资源类型（场景、普通资源、音频）封装专属加载入口方法，比如：
//LoadSceneAssetsAsync()：适配场景加载（调用 Addressables.LoadSceneAsync）；
//LoadAudioAssetsAsync()：适配音频加载（补充 LoadAudioData）；
//LoadNormalAssetsAsync()：复用你现有代码，适配预制 / 图片等普通多资源加载；
//所有专属层方法最终都调用基础层的通用逻辑，只在 “加载 API 选择”“回调后处理” 上做差异化。
//二、核心思路 2：统一缓存 Key 生成（解决 Key 重复 / 混乱）
//你当前的 Key 是 “拼接 key + 类型名”，易重复 / 过长，需适配不同资源类型做优化：
//加资源类型前缀：给不同资源类型的 Key 加固定前缀（比如场景：Scene_、音频：Audio_、普通资源：Asset_），避免不同类型资源的 Key 冲突；
//多资源 Key 优化：用「哈希（MD5/SHA1）」替代简单字符串拼接（比如把 list 里的 key 排序后拼接，再哈希），避免 “key 顺序不同但内容相同” 导致生成不同 Key（比如 ["A","B"] 和 ["B","A"] 生成同一 Key）；
//缓存时记录资源类型：assetDic 不只是存 Handle，而是存「Handle + 资源类型枚举（Scene/Audio/Normal）」的结构体，方便后续卸载 / 处理时识别类型。
//三、核心思路 3：适配不同资源类型的专属加载 API（核心差异化）
//不同资源的加载 API 完全不同，需在分支层做 API 选择和逻辑补充：
//资源类型	专属适配逻辑
//场景	1. 调用 Addressables.LoadSceneAsync（而非 LoadAssetsAsync），补充 LoadSceneMode（Single/Additive）参数；
//2. 回调后自动处理场景激活 / 叠加；
//3. 缓存时记录 SceneInstance，方便后续卸载；
//音频	1. 复用 LoadAssetsAsync，但在action?.Invoke(obj)后补充AudioClip.LoadAudioData()；
//2. 预留音频数据释放钩子（UnloadAudioData）；
//普通资源（预制 / 图片）	完全复用你现有代码，无额外逻辑；
//四、核心思路 4：补充统一的资源生命周期管理（适配卸载 / 释放）
//你现有代码只做了加载缓存，未处理卸载，需适配不同资源的专属卸载逻辑：
//统一卸载入口：新增UnloadAssetsAsync(string cacheKey)方法，根据缓存中记录的「资源类型」调用对应卸载 API：
//场景：Addressables.UnloadSceneAsync；
//音频：先调用 AudioClip.UnloadAudioData ()，再 Release Handle；
//普通资源：Addressables.Release (handle)；
//自动清理机制：给缓存的 Handle 加 “引用计数”，场景切换 / 资源不用时，引用计数归 0 则自动触发卸载，避免内存泄漏；
//失败后资源清理：加载失败时，除了删除缓存，还要尝试释放已加载的部分资源（比如场景加载一半失败，清理临时场景实例）。
//五、核心思路 5：增强容错 + 预留扩展钩子（适配未来场景）
//容错增强：
//加 “加载中” 状态标记：避免同一 Key 被重复发起加载（比如加载中时，新的加载请求直接监听已有 Handle 的 Completed，不新建）；
//扩展状态判断：除了 IsDone/Status，增加对 Handle.Valid () 的判断，避免操作无效 Handle；
//扩展钩子：在加载的关键节点预留可自定义的委托钩子，无需修改核心逻辑即可适配新资源类型：
//OnBeforeLoad：加载前钩子（比如场景加载前检查依赖）；
//OnAfterLoad：加载完成后钩子（比如音频的 LoadAudioData、场景的激活）；
//OnLoadFailed：失败后自定义处理钩子；
//六、核心思路 6：兼容单 / 多资源加载（补充灵活性）
//你当前代码只适配多资源（LoadAssetsAsync），需补充单资源加载的重载，共用缓存逻辑：
//新增LoadAssetAsync<T>(string key, Action<T> action)重载方法，调用 Addressables.LoadAssetAsync，生成单资源的缓存 Key，复用你现有代码的缓存 / 状态判断逻辑；
//统一回调格式：单资源回调直接传 T，多资源回调传 IList<T>，让上层调用无需区分单 / 多。
//总结（思路核心回顾）
//分层：通用基础层（缓存 / 状态）+ 资源类型专属层（API / 回调处理），保留你现有代码的核心；
//标识：统一 Key 生成（加类型前缀 + 哈希），缓存时记录资源类型；
//适配：不同资源调用专属加载 API，补充类型特有逻辑（场景的 LoadSceneMode、音频的 LoadAudioData）；
//生命周期：统一卸载入口，按资源类型处理释放逻辑；
//扩展：加状态容错 + 钩子，适配未来新增资源类型。
//这套思路的核心是「保留你的核心逻辑不变，只做 “分层分支” 和 “专属补充”」，既适配所有加载场景，又不破坏你现有代码的结构。一、核心思路 1：分层抽象（通用层 + 资源类型专属层）

public class AddressableMgr : Singleton<AddressableMgr>
{
    public Dictionary<string, IEnumerator> assetDic = new Dictionary<string,IEnumerator>();
    protected override void Awake()
    {
        
    }

    private string CreateCacheKey<T>(IEnumerable<string> keys)
    {
        StringBuilder sB = new StringBuilder();
        List<string> sortedKeys = new List<string>(keys);
        sortedKeys.Sort ();
        foreach(string key in sortedKeys)       
        {
            sB.Append (key);
            sB.Append ("_");//?
        }
        sB.Append (typeof(T).Name);
        return sB.ToString ();
    }

    private void HandleLoadFailed(string keyName)
    {
        Debug.LogError ($"{keyName} 资源加载异常");
        if(assetDic.ContainsKey (keyName))
        {
            assetDic.Remove (keyName);
        }
    }

    public void LoadAssetAsync<T>(Action<T> action,string key)
    {
        string keyName = $"{key}_{ typeof (T).Name}";
                
        AsyncOperationHandle<T> handle;
         
        if(assetDic.ContainsKey(keyName))
        {
            handle = (AsyncOperationHandle<T>)assetDic[keyName];
            if(handle.IsDone)
            {
                if(handle.Status == AsyncOperationStatus.Succeeded)
                {
                    action?.Invoke (handle.Result);
                }
                else
                {
                    HandleLoadFailed (keyName);
                }
            }
            else
            {
                handle.Completed += (handlecallback) =>
                {
                    if(handlecallback.Status == AsyncOperationStatus.Succeeded)
                    {
                        action?.Invoke (handlecallback.Result);
                    }
                    else
                    {
                        HandleLoadFailed (keyName);
                    }
                };
            }
            return;
        }

        handle = Addressables.LoadAssetAsync<T> (key);

        handle.Completed += (handlecallback) =>
        {
            if(handlecallback.Status == AsyncOperationStatus.Succeeded)
            {
                action?.Invoke (handlecallback.Result);
            }
            else
            {
                HandleLoadFailed (keyName);
            }
        };
        assetDic.Add (keyName, handle);
    }

    public void LoadAssetAsync<T>(Action<T> action , Addressables.MergeMode mode, params string[] keys)
    {
        string keyName = CreateCacheKey<T>(keys);

        AsyncOperationHandle<IList<T>> handle;

        if(assetDic.ContainsKey (keyName))
        {
            handle = (AsyncOperationHandle<IList<T>>)assetDic[keyName];
            if(handle.IsDone)
            {
                foreach(T obj in handle.Result)
                {
                    action?.Invoke (obj);
                }
            }  
            else
            {
                handle.Completed += (handlecallback) =>
                {
                    if(handlecallback.Status == AsyncOperationStatus.Succeeded)
                    {
                        foreach(T obj in handlecallback.Result)
                        {
                            action?.Invoke (obj);
                        }
                    }
                };
            }
            return; 
        }

        List<string> list = new List<string> (keys);
        handle = Addressables.LoadAssetsAsync<T> (list,action, mode);
        handle.Completed += (handlecallback) =>
        {
            if(handlecallback.Status == AsyncOperationStatus.Failed)
            {
                HandleLoadFailed (keyName);
            }
        };
        assetDic.Add(keyName, handle);
    }

    public void LoadAudioAssetAsync(Action<AudioClip> action, Addressables.MergeMode mode, params string[] keys)
    {
        LoadAssetAsync<AudioClip> (audio =>
        {
            if(audio != null)
            {
                audio.LoadAudioData ();
            }
            action?.Invoke (audio);
        },mode,keys);
    }

    public void LoadSceneAssetAsync(Action<SceneInstance> action,LoadSceneMode sceneMode = 0,  params string[] key)
    {
        string keyName = CreateCacheKey<SceneInstance> (key);

        AsyncOperationHandle<SceneInstance> handle;

        if(assetDic.ContainsKey (keyName))
        {
            handle = (AsyncOperationHandle<SceneInstance>)assetDic[keyName];
            if(handle.IsDone)
            {
                if(handle.Status == AsyncOperationStatus.Succeeded)
                {
                    action?.Invoke (handle.Result);
                }
                else
                {
                    HandleLoadFailed (keyName);
                }
            }
            else
            {
                handle.Completed += (handlecallback) =>
                {
                    if(handlecallback.Status == AsyncOperationStatus.Succeeded)
                    {
                        action?.Invoke (handlecallback.Result);
                    }
                    else
                    {
                        HandleLoadFailed (keyName);
                    }
                };
            }
            return;
        }

        handle = Addressables.LoadSceneAsync(key[0], sceneMode);

        handle.Completed += (handlecallback) =>
        {
            if(handlecallback.Status == AsyncOperationStatus.Succeeded)
            {
                action?.Invoke (handlecallback.Result);
            }
            else
            {
                HandleLoadFailed (keyName);
            }
        };
        assetDic.Add (keyName, handle);
    }

    public void Clear()
    {
        foreach(var handle in assetDic.Values)
        {
            Addressables.Release (handle);
        }

        // 清空缓存字典
        assetDic.Clear ();

        // 卸载未使用的资源（可选）
        Resources.UnloadUnusedAssets ();
        GC.Collect ();
    }

    public void Release<T>(string name)
    {
        string keyName = $"{name}_{typeof (T).Name}";
        if(assetDic.ContainsKey (keyName))
        {
            AsyncOperationHandle<T> handle = (AsyncOperationHandle<T>)assetDic[keyName];
            if(handle.IsValid ())
            {
                Addressables.Release (handle); // 释放资源
            }
            assetDic.Remove (keyName);
        }
    }

    // 多资源释放
    public void Release<T>(params string[] key)
    {
        string keyName = CreateCacheKey<T> (key);
        if(assetDic.ContainsKey (keyName))
        {
            AsyncOperationHandle<IList<T>> handle = (AsyncOperationHandle<IList<T>>)assetDic[keyName];
            if(handle.IsValid ())
            {
                Addressables.Release (handle);
            }
            assetDic.Remove (keyName);
        }
    }

    // 场景资源释放
    public void ReleaseScene(params string[] key)
    {
        string keyName = CreateCacheKey<SceneInstance> (key);
        if(assetDic.ContainsKey (keyName))
        {
            AsyncOperationHandle<SceneInstance> handle = (AsyncOperationHandle<SceneInstance>)assetDic[keyName];
            if(handle.IsValid ())
            {
                Addressables.UnloadSceneAsync (handle); // 场景专属释放
                Addressables.Release (handle);
            }
            assetDic.Remove (keyName);
        }
        Clear ();
    }

    //public void LoadAssetAsync<T>(string name, Action<AsyncOperationHandle<T>> action)弃用
    //{
    //    string keyName = name + "_" + typeof(T).Name;

    //    AsyncOperationHandle<T> handle;

    //    if(assetDic.ContainsKey(keyName))
    //    {
    //        handle = (AsyncOperationHandle<T>)assetDic[keyName];
    //        if(handle.IsDone)
    //        {
    //            if(handle.Status == AsyncOperationStatus.Succeeded)
    //            {
    //                action ?.Invoke (handle);
    //            }
    //            else
    //            {
    //                assetDic.Remove(keyName);
    //                Debug.LogWarning (keyName + "资源加载异常");
    //            }
    //        }
    //        else
    //        {
    //            handle.Completed += (obj) =>
    //            {
    //                if(obj.Status == AsyncOperationStatus.Succeeded)
    //                {
    //                    action?.Invoke (obj);
    //                }
    //                else
    //                {
    //                    if(assetDic.ContainsKey (keyName))
    //                        assetDic.Remove (keyName);
    //                    Debug.LogWarning (keyName + "资源加载异常");
    //                }
    //            };
    //        }
    //        return;
    //    }

    //    handle = Addressables.LoadAssetAsync<T> (name);
    //    handle.Completed += (handlecallback) =>
    //    {
    //        if(handlecallback.Status == AsyncOperationStatus.Succeeded)
    //        {
    //            action ?.Invoke (handlecallback);
    //        }
    //        else
    //        {
    //            if(assetDic.ContainsKey(keyName)) 
    //                assetDic.Remove (keyName);
    //            Debug.LogWarning (keyName + "资源加载异常");
    //        }

    //    };
    //    assetDic.Add(keyName, handle);
    //}

    //public void LoadAssetAsync<T>(Addressables.MergeMode mode, Action<AsyncOperationHandle<IList<T>>> action, params string[] key)弃用
    //{
    //    List<string> list = new List<string> (key);
    //    StringBuilder sB = new StringBuilder ();

    //    foreach(string keys in list)
    //    {
    //        sB.Append (keys);
    //        sB.Append ("_");
    //    }
    //    sB.Append (typeof (T).Name);
    //    string keyName = sB.ToString ();
    //    AsyncOperationHandle<IList<T>> handle;

    //    if(assetDic.ContainsKey (keyName))
    //    {
    //        handle = (AsyncOperationHandle<IList<T>>)assetDic[keyName];
    //        if(handle.IsDone)
    //        {
    //            action?.Invoke (handle);
    //        }
    //        else
    //        {
    //            handle.Completed += (handlecallback) =>
    //            {
    //                if(handlecallback.Status == AsyncOperationStatus.Succeeded)//?
    //                {
    //                    action?.Invoke (handlecallback);
    //                }
    //            };
    //        }
    //        return;
    //    }
    //    handle = Addressables.LoadAssetsAsync<T> (list, null, mode);

    //    handle.Completed += (handlecallback) =>
    //    {
    //        if(handle.Status == AsyncOperationStatus.Succeeded)
    //        {
    //            action?.Invoke (handlecallback);
    //        }
    //        else if(handlecallback.Status == AsyncOperationStatus.Failed)
    //        {
    //            Debug.LogError (keyName + "资源加载异常");
    //            if(assetDic.ContainsKey (keyName))
    //            {
    //                assetDic.Remove (keyName);
    //            }
    //        }
    //    };
    //    assetDic.Add (keyName, handle);
    //}

    //public void Release<T>(string name)
    //{
    //    string keyName = name + "_" + typeof (T).Name;
    //    if(assetDic.ContainsKey (keyName))
    //    {
    //        AsyncOperationHandle<T> handle = (AsyncOperationHandle<T>)assetDic[keyName];
    //        Addressables.Release(handle);
    //        assetDic.Remove (keyName);
    //    }
    //}

    //public void Release<T>(params string[] key)
    //{
    //    List<string> list = new List<string> (key);
    //    StringBuilder sB = new StringBuilder ();

    //    foreach(string keys in list)
    //    {
    //        sB.Append (keys);
    //        sB.Append ("_");
    //    }
    //    sB.Append (typeof (T).Name);
    //    string keyName = sB.ToString ();

    //    if(assetDic.ContainsKey (keyName))
    //    {
    //        AsyncOperationHandle<IList<T>> handle = (AsyncOperationHandle<IList<T>>)assetDic[keyName];
    //        Addressables.Release(handle);
    //        assetDic.Remove (keyName);
    //    }
    //}

}
