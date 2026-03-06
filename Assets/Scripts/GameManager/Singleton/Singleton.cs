using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class SingletonC<T> where T : SingletonC<T> , new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }
}

public class Singleton<T> : MonoBehaviour where T : Singleton<T>//T来自子类继承
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                
                instance = FindObjectOfType<T>();
                if(instance == null)
                {
                    GameObject gameObject = GameObject.Find ("GlobleSingletonEntrance");
                    if(gameObject == null)
                    { 
                        gameObject = new GameObject ("GlobleSingletonEntrance");
                        DontDestroyOnLoad (gameObject);
                    }
                    instance = gameObject.AddComponent<T> ();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if(instance == null)
        {
            instance = (T)this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

public class SingletonLocal<T>:MonoBehaviour where T : SingletonLocal<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject gameObject = GameObject.Find ("LocalSingletonEntrance");
                if(gameObject == null)
                {
                    gameObject = new GameObject ("LocalSingletonEntrance");
                    DontDestroyOnLoad (gameObject);
                }
                instance = gameObject.AddComponent<T> ();
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if(instance == null)
        {
            instance = (T)this;
        }
        else
        {
            Destroy (gameObject);
        }
    }
}

public interface MUniversalInterface
{

}