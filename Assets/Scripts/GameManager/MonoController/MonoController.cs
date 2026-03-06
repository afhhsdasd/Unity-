using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoController : Singleton<MonoController>
{
    //事件订阅推拉结合，
    public event UnityAction updateEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateEvent?.Invoke();
    }

    public void AddUpdateListener(UnityAction func)
    {
        updateEvent += func;

    }

    public void RemoveUpdateListener(UnityAction func)
    {
        updateEvent -= func;
    }

    public T GetInstantiate<T>(T prefeb) where T : Object
    {
        return Instantiate<T>(prefeb);
    }

}
