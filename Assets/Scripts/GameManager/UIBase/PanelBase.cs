using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelBase: MonoBehaviour
{
    protected Dictionary<string, List<UIBehaviour>> controllerDic = new Dictionary<string, List<UIBehaviour>> ();

    protected virtual void Awake()
    {
        FindChildController<Button> ();
        FindChildController<TextMeshProUGUI> ();
        FindChildController<Text>();
        FindChildController<Image> ();
        FindChildController<Slider> ();
        FindChildController<Toggle> ();
        FindChildController<ScrollRect> ();
        FindChildController<InputField> ();
    }

    public void FindChildController<T>() where T : UIBehaviour
    {
        T[] controller = gameObject.GetComponentsInChildren<T> (true);

        foreach(var control in controller)
        {
            if(controllerDic.ContainsKey (control.name))
            {
                controllerDic[control.name].Add (control);
            }
            else
            {
                controllerDic.Add (control.name,new List<UIBehaviour> () { control });
            }

            if(control is Button)
            {
                (control as Button).onClick.AddListener (() =>
                {
                    OnClick (control.name);
                });
            }
            else if (control is Toggle)
            {
                (control as Toggle).onValueChanged.AddListener ((state) =>
                {
                    ToggleOnValueChanged (control.name, state);
                });
            }
            else if(control is Slider)
            {
                (control as Slider).onValueChanged.AddListener ((value) =>
                {
                    SliderOnValueChanged (control.name, value);
                });
            }
        }
    }

    public T GetControl<T>(string name) where T : UIBehaviour
    {
        if(controllerDic.ContainsKey (name))
        {
            foreach(var control in controllerDic[name])
            {
                if(control is T)
                {
                    //仅返回一次，返回第一个
                    return (T)control;
                }
            }
        }
        return null;
    }
    public void AddCustomListener(UIBehaviour controller, EventTriggerType type, UnityAction<BaseEventData> callback)
    {//自定义控件
        EventTrigger trigger = controller.GetComponent<EventTrigger> ();
        if(trigger == null)
        {
            trigger = controller.gameObject.AddComponent<EventTrigger> ();

        }
        //装一个操作响应器
        EventTrigger.Entry entry = new EventTrigger.Entry ();
        entry.eventID = type;
        entry.callback.AddListener (callback);

        trigger.triggers.Add (entry);
    }

    public virtual void OnClick(string name)
    {

    }

    public virtual void ToggleOnValueChanged(string name,bool state)
    {

    }

    public virtual void SliderOnValueChanged(string name, float state)
    {

    }

}
