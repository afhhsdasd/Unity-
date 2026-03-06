using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PanelBegin : PanelBase
{
    
    protected override void Awake()
    {
        base.Awake ();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnClick(string name)
    {
        
        switch(name)
        {
            case "ButtonBegin":
                //
                break;
            case "ButtonSetting":
                UIMgr.Instance.ShowPanel<PanelSetting> ("PanelSetting");
                break;
            case "ButtonQuit":
                if(EditorUtility.DisplayDialog ("退出游戏", "确定要退出吗？", "确定", "取消"))
                {
                    Application.Quit();
                }
                break;
        }
    }
}
