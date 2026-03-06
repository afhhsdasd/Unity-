using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*1. 音量设置（背景音乐 / 音效，最基础且高频）
2.画质等级（低 / 中 / 高，体现性能优化意识，面试加分）
3. 全屏 / 窗口切换（PC 端必备）
4. 重置默认设置（实用功能）
5. 语言切换（简易版，拓展性强）
Slider_BGM：背景音乐滑块（范围 0-1）
Slider_SFX：音效滑块（范围 0-1）
Toggle_FullScreen：全屏开关
Dropdown_Quality：画质下拉框（选项：Low/Medium/High）
Dropdown_Language：语言下拉框（选项：中文 / English）
Button_Reset：重置设置按钮*/
public class PanelSetting : PanelBase
{
    protected override void Awake()
    {
        base.Awake();
        GetControl<Slider> ("SliderMusic").value = FoundationData.Instance.music;
        GetControl<Slider> ("SliderSound").value = FoundationData.Instance.sound;
    }

    public override void OnClick(string name)
    {
        switch(name)
        {
            case "ButtonCancel" :
                UIMgr.Instance.HidePanel("PanelSetting");
                break;

            case "ButtonConfirm":
                UIMgr.Instance.HidePanel("PanelSetting");
                FoundationData.Instance.SaveDate();
                break;

            case "":
                break;
            //case "":
            //    break;
            //case "":
            //    break;
            //case "":
            //    break;
            //case "":
            //    break;
            //case "":
            //    break;
            //case "":
            //    break;
        }
    }

    public override void ToggleOnValueChanged(string name, bool state)
    {
        switch(name)
        {
            case "":
                break;
            //case "":
            //    break;
            //case "":
            //    break;
        }
    }

    public override void SliderOnValueChanged(string name, float value)
    {
        switch(name)
        {
            case "":
                break;
            case "SliderMusic":
                FoundationData.Instance.music = value;
                break;
            case "SliderSound":
                FoundationData.Instance.sound = value;
                break;
        }
    }
}
