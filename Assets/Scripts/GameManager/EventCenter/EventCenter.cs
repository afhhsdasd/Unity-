using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class EventCenter : Singleton<EventCenter>
{//类中数据结构类，不放在外部，因为外部不需要这种数据结构


    public class EventInfo<T>:MUniversalInterface
    {
        public UnityAction<T> unityAction;

        public EventInfo(UnityAction<T> action)
        {
            unityAction = action;
        }

    }
    public class EventInfo:MUniversalInterface
    {
        public UnityAction unityAction;

        public EventInfo(UnityAction action)
        {
            unityAction = action;
        }

    }
    //逻辑部分
    private Dictionary<string, MUniversalInterface> eventDic = new Dictionary<string, MUniversalInterface> ();
    //怪物奖励1，怪物奖励2，怪物死亡，怪物脚本调用invoke，
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if(eventDic.ContainsKey (eventName))
        {
            
            ((EventInfo<T>)eventDic [eventName]).unityAction += action;
        }
        else
        {
            eventDic.Add (eventName, new EventInfo<T> (action));
            /*构造函数类型就是类的类型，通过new找到类来调用构造函数,
             构造函数返回值是相应类型的引用并自动关联实例，用这个引用改字段
            调用方法都会破坏返回类型，我必须在这个方法体内修改this的属性，
            这叫初始化，然后将this返回出去*/
        }
    }
    public void AddEventListener(string eventName, UnityAction action)
    {
        if(eventDic.ContainsKey (eventName))
        {

            ((EventInfo)eventDic[eventName]).unityAction += action;
        }
        else
        {
            eventDic.Add (eventName, new EventInfo (action));
        }
    }

    public void EventTrigger<T>(string eventName,T obj)
    {
        if(eventDic.ContainsKey(eventName))
        {
            ((EventInfo<T>)eventDic[eventName]).unityAction?.Invoke (obj);
        }
    }
    public void EventTrigger(string eventName)
    {
        if(eventDic.ContainsKey (eventName))
        {
            ((EventInfo)eventDic[eventName]).unityAction?.Invoke ();
        }
    }

    public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
    {
        if( eventDic.ContainsKey (eventName))
        {
            ((EventInfo<T>)eventDic[eventName]).unityAction -= action;
        }
    }
    public void RemoveEventListener(string eventName, UnityAction action)
    {
        if(eventDic.ContainsKey (eventName))
        {
            ((EventInfo)eventDic[eventName]).unityAction -= action;
        }
    }
    
    public void clear()
    {
        eventDic.Clear ();
    }
}