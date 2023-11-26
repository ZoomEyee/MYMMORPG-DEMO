using LitJson;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public enum JsonType
{
    JsonUtlity,
    LitJson,
    NewtonsoftJson,
}
public class JsonDataMgr : Singleton<JsonDataMgr>
{
    public string JSON_PER_PATH = Application.persistentDataPath + "/Json/";
    public string JSON_STR_PATH = Application.streamingAssetsPath + "/ExcelData/Json/";
    public void SaveData<T>(string fileName, T data, bool isStreaming = false, JsonType type = JsonType.NewtonsoftJson) where T : class, new()
    {
        string path = isStreaming ? JSON_STR_PATH + fileName + ".json" : JSON_PER_PATH + fileName + ".json";
        if (!File.Exists(JSON_PER_PATH) || !File.Exists(JSON_STR_PATH))
        {
            Directory.CreateDirectory(JSON_PER_PATH);
            Directory.CreateDirectory(JSON_STR_PATH);
        }
        string jsonStr = "";
        switch (type)
        {
            case JsonType.NewtonsoftJson:
                jsonStr = JsonConvert.SerializeObject(data);
                break;
            case JsonType.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;           
        }
        File.WriteAllText(path, jsonStr);
    }
    public T LoadData<T>(string fileName, bool isStreaming = false, JsonType type = JsonType.NewtonsoftJson) where T : class, new()
    {
        string path = isStreaming ? JSON_STR_PATH + fileName + ".json" : JSON_PER_PATH + fileName + ".json";
        if (!File.Exists(path))
            return new T();

        string jsonStr = File.ReadAllText(path);
        T data = default(T);
        switch (type)
        {
            case JsonType.NewtonsoftJson:
                data = JsonConvert.DeserializeObject<T>(jsonStr);
                break;
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
            
        }
        return data;
    }
}
