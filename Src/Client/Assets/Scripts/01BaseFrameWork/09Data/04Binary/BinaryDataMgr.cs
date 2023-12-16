using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class BinaryDataMgr : Singleton<BinaryDataMgr>
{
    public string BINARY_PER_PATH = Application.persistentDataPath  + "/Binary/";
    public string BINARY_STR_PATH = Application.streamingAssetsPath + "/ExcelData/Binary/";
    public string EXTENSION_NORMAL = ".czkn";
    public string EXTENSION_EXCEL = ".czke";

    private Dictionary<string, object> tableDic = new Dictionary<string, object>();
    public void SaveData<T>(string fileName, T obj) where T : class
    {
        if (!Directory.Exists(BINARY_PER_PATH))
            Directory.CreateDirectory(BINARY_PER_PATH);

        using (FileStream fs = new FileStream(BINARY_PER_PATH + fileName + EXTENSION_NORMAL, FileMode.OpenOrCreate, FileAccess.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, obj);
            fs.Close();
        }
    }
    public T LoadData<T>(string fileName) where T : class
    {
        string path = BINARY_PER_PATH + fileName + EXTENSION_NORMAL;
        if (!File.Exists(path))
            return default(T);
        T obj;
        using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(fs) as T;
            fs.Close();
        }
        return obj;
    }
    public CONTAINER LoadTable<CLASS, CONTAINER>() where CONTAINER : class
    {
        if (tableDic.ContainsKey(typeof(CONTAINER).Name))
            return tableDic[typeof(CONTAINER).Name] as CONTAINER;
        using (FileStream fs = File.Open(BINARY_STR_PATH + typeof(CONTAINER).Name + EXTENSION_EXCEL, FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            int index = 0;

            int containerCount = BitConverter.ToInt32(bytes, index);
            index += 4;
            int keyNameLength = BitConverter.ToInt32(bytes, index);
            index += 4;
            string keyName = Encoding.UTF8.GetString(bytes, index, keyNameLength);
            index += keyNameLength;
                        
            object containerObj = Activator.CreateInstance(typeof(CONTAINER));                     
            FieldInfo[] classFields = typeof(CLASS).GetFields();
            FieldInfo[] containerFields = typeof(CONTAINER).GetFields();

            object keyValue;
            object dicObj = containerFields[0].GetValue(containerObj);
            MethodInfo mInfo = dicObj.GetType().GetMethod("Add");
            
            for (int i = 0; i < containerCount; i++)
            {
                object classObj = Activator.CreateInstance(typeof(CLASS));
                foreach (FieldInfo info in classFields)
                {
                    if (info.FieldType == typeof(int))
                    {
                        info.SetValue(classObj, BitConverter.ToInt32(bytes, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(float))
                    {
                        info.SetValue(classObj, BitConverter.ToSingle(bytes, index));
                        index += 4;
                    }
                    else if (info.FieldType == typeof(bool))
                    {
                        info.SetValue(classObj, BitConverter.ToBoolean(bytes, index));
                        index += 1;
                    }
                    else if (info.FieldType == typeof(string))
                    {
                        int length = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        info.SetValue(classObj, Encoding.UTF8.GetString(bytes, index, length));
                        index += length;
                    }
                }
                keyValue = typeof(CLASS).GetField(keyName).GetValue(classObj);
                mInfo.Invoke(dicObj, new object[] { keyValue, classObj });
            }
            tableDic.Add(typeof(CONTAINER).Name, containerObj);
            fs.Close();
            return containerObj as CONTAINER;
        }
    }
    public void ClearDic()
    {
        tableDic.Clear();
    }


}

