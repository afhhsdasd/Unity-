using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum E_UI_Layer
{
    Bottom,
    Middle,
    Top,
    System,
}
public class UIMgr : Singleton<UIMgr>
{
    public RectTransform canvas;
    public Transform bottom;
    public Transform middle;
    public Transform top;
    public Transform system;

    public Dictionary<string,PanelBase> panelDic = new Dictionary<string,PanelBase>();

    protected override void Awake()
    {
        base.Awake();
        AddressableMgr.Instance.LoadAssetAsync<GameObject> (callback =>
        {
            GameObject canvasInstance = GameObject.Instantiate (callback);
            bottom = canvasInstance.transform.Find ("Bottom");
            middle = canvasInstance.transform.Find ("Middle");
            top = canvasInstance.transform.Find ("Top");
            system = canvasInstance.transform.Find ("System");
            this.canvas = (RectTransform)canvasInstance.transform;
            DontDestroyOnLoad(canvasInstance);
        }, Addressables.MergeMode.Intersection,"Canvas","UI");
        
    }

    private IEnumerator Eavesdropcanvas(Action endwatting)
    {
        while(canvas == null)
        {
            yield return null;
        }
        endwatting();
    }
    private IEnumerator DelayRebuildLayout(RectTransform rect)
    {
        yield return null; // 等1帧，让父节点尺寸生效
        LayoutRebuilder.ForceRebuildLayoutImmediate (rect);
    }

    public void ShowPanel<T>(string panelName,E_UI_Layer layer = E_UI_Layer.Middle, Action<T> callBack = null) where T : PanelBase
    {//委托变量参数=null，设为可选参数 ?是否加入alpha通道

        if(panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].gameObject.SetActive(true);
            callBack?.Invoke ((T)panelDic[panelName]);
        }
        else
        {
            StartCoroutine (Eavesdropcanvas (()=>{
                AddressableMgr.Instance.LoadAssetAsync <GameObject> ( panelcallback =>
                {
                    if(panelDic.ContainsKey (panelName))
                    {
                        //panelDic[panelName].gameObject.SetActive (true);
                        return;
                    }
                    Transform father = canvas;
                    switch(layer)
                    {
                        case E_UI_Layer.Bottom:
                            father = bottom;
                            break;
                        case E_UI_Layer.Middle:
                            father = middle;
                            break;
                        case E_UI_Layer.Top:
                            father = top;
                            break;
                        case E_UI_Layer.System:
                            father = system;
                            break;
                    }
                    GameObject panel = Instantiate (panelcallback,father);
                    //作为canvas子对象并设置相对位置
                    
                    //panel.transform.SetParent (father,false);

                    panel.transform.localPosition = Vector3.zero;
                    panel.transform.localScale = Vector3.one;

                    ((RectTransform)panel.transform).offsetMax = Vector2.zero;
                    ((RectTransform)panel.transform).offsetMin = Vector2.zero;

                    T panelInform = panel.GetComponent<T>();
                    callBack?.Invoke (panelInform);

                    panel.name = panelName;
                    panelDic.Add (panelName, panelInform);
                },Addressables.MergeMode.Intersection,panelName,"UI");
            }));
        }
    }
    public void HidePanel(string panelName)
    {
        if(panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].gameObject.SetActive(false);
        }
    }

    //public Transform GetPanel(E_UI_Layer layer)
    //{
    //    switch(layer)
    //    {
    //        case E_UI_Layer.Bottom:
    //            break;
    //        case E_UI_Layer.Middle:
    //            break;
    //        case E_UI_Layer.Top:
    //            break;
    //        case E_UI_Layer.System:
    //            break;
    //    }
    //    return null;
    //}

    public T GetPanel<T>(string panelName) where T: PanelBase
    {
        if (panelDic.ContainsKey (panelName))
        {
            return (T)panelDic[panelName];
        }
        return null;
    }

}
