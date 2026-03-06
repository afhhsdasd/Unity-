//using LitJson;
//using Newtonsoft.Json;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//public enum JsonType
//{
//    JsonUtlity,
//    LitJson,
//    Newton
//}
//public class JsonMgr
//{
//    private static JsonMgr instance = new JsonMgr();
//    public static JsonMgr Instance => instance;

//    private JsonMgr() { }

//    public void SaveData(object data,string fileName,JsonType type = JsonType.Newton)
//    {
//        string path = Application.persistentDataPath + "/" + fileName + ".json";

//        string jsonStr = "";
//        switch(type)
//        {
//            case JsonType.JsonUtlity:
//                jsonStr = JsonUtility.ToJson(data);
//                break;
//            case JsonType.LitJson:
//                jsonStr = JsonMapper.ToJson(data);
//                break;
//            case JsonType.Newton:
//                jsonStr = JsonConvert.SerializeObject (data);
//                break;
//        }
//        File.WriteAllText(path, jsonStr); 
//    }

//    public T LoadData<T>(string fileName, JsonType type = JsonType.Newton) where T : new()
//    {
//        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        
//        if(File.Exists (path))
//        {
//            path = Application.persistentDataPath + "/" + fileName + ".json";
//            if(!File.Exists(path))
//            {
//                File.Copy (Application.streamingAssetsPath + "/" + fileName + ".json", path);
//            }
//        }
//        if(!File.Exists (path))
//        {
//            return new T();
//        }

//        string jsonStr = File.ReadAllText (path);


//        T data = default(T);
//        switch(type)
//        {
//            case JsonType.JsonUtlity:
//                data = JsonUtility.FromJson<T>(jsonStr);
//                break;
//            case JsonType.LitJson:
//                data = JsonMapper.ToObject<T>(jsonStr);
//                break;
//            case JsonType.Newton:
//                data = JsonConvert.DeserializeObject<T> (jsonStr); ;
//                break;
//        }

//        return data;

//    }
//}

///
using Newtonsoft.Json;
using LitJson;
using System;
using System.IO;
using UnityEngine;

public enum JsonType
{
    JsonUtlity,
    LitJson,
    Newton
}

public class JsonMgr : Singleton<JsonMgr>
{
    // 单例：加volatile保证线程安全（Unity主线程用也可忽略，但更规范）
    //private static volatile JsonMgr instance;
    //public static JsonMgr Instance
    //{
    //    get
    //    {
    //        if(instance == null)
    //        {
    //            instance = new JsonMgr ();
    //        }
    //        return instance;
    //    }
    //}

    //private JsonMgr() { }

    /// <summary>
    /// 保存数据（修正：加目录创建+Newton格式化+统一默认值）
    /// </summary>
    public void SaveData(object data, string fileName, JsonType type = JsonType.Newton)
    {
        try
        {
            
            string path = Path.Combine (Application.persistentDataPath, $"{fileName}.json");
            Directory.CreateDirectory (Path.GetDirectoryName (path));

            string jsonStr = "";
            switch(type)
            {
                case JsonType.JsonUtlity:
                    jsonStr = JsonUtility.ToJson (data, true); // 格式化
                    break;
                case JsonType.LitJson:
                    jsonStr = JsonMapper.ToJson (data);
                    break;
                case JsonType.Newton:
                    //格式化
                    jsonStr = JsonConvert.SerializeObject (data, Formatting.Indented);
                    break;
            }

            File.WriteAllText (path, jsonStr);
            //Debug.Log ($"数据保存成功：{path}");
        }
        catch(Exception e)
        {
            Debug.LogError ($"数据保存失败：{e.Message}");
        }
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    public T LoadData<T>(string fileName, JsonType type = JsonType.Newton) where T : new()
    {
        try
        {
            string persistentPath = Path.Combine (Application.persistentDataPath, $"{fileName}.json");
            string streamingPath = Path.Combine (Application.streamingAssetsPath, $"{fileName}.json");

            if(File.Exists (persistentPath))
            {
                string jsonStr = File.ReadAllText (persistentPath);
                return Deserialize<T> (jsonStr, type);
            }

            if(File.Exists (streamingPath))
            {
                // 复制初始数据到PersistentDataPath,没有怎么办？
                File.Copy (streamingPath, persistentPath, true);
                string jsonStr = File.ReadAllText (streamingPath);
                return Deserialize<T> (jsonStr, type);
            }

            //Debug.LogWarning ($"未找到数据文件，返回默认值：{fileName}.json");
            return new T ();
        }
        catch(Exception e)
        {
            Debug.LogError ($"数据加载失败：{e.Message}");
            return new T ();
        }
    }

    /// <summary>
    /// 抽离反序列化逻辑
    /// </summary>
    private T Deserialize<T>(string jsonStr, JsonType type) where T : new()
    {
        T data = new(); // 创建实例，避免null
        switch(type)
        {
            case JsonType.JsonUtlity:
                JsonUtility.FromJsonOverwrite (jsonStr, data);
                break;
            case JsonType.LitJson:
                data = JsonMapper.ToObject<T> (jsonStr);
                break;
            case JsonType.Newton:
                data = JsonConvert.DeserializeObject<T> (jsonStr);
                break;
        }
        return data;
    }
}