using Newtonsoft.Json;
using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[Serializable]
public class FoundationData
{
    [JsonIgnore]
    private static FoundationData instance;
    [JsonIgnore]
    public static FoundationData Instance
    {
        get 
        {
            if(instance == null)
            {
                instance = JsonMgr.Instance.LoadData<FoundationData> ("FoundionData");
                Application.quitting += () => { instance.SaveDate (); Application.quitting -= instance.SaveDate; };
            }
            return instance;
        }
    }

    public float music;
    public float sound;


    public void SaveDate()
    {
        JsonMgr.Instance.SaveData(instance , "FoundationData");
    }

}