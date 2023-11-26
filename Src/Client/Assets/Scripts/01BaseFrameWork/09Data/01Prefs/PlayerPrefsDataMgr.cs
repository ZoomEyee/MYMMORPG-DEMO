using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlayerPrefsDataMgr : Singleton<PlayerPrefsDataMgr>
{
    public void SaveData(string keyName, object data)
    {
        Type dataType = data.GetType();
        FieldInfo[] infos = dataType.GetFields();
        string saveKeyName;
        FieldInfo info;
        for (int i = 0; i < infos.Length; i++)
        {
            info = infos[i];
            saveKeyName = keyName + "_" + dataType.Name +
                "_" + info.FieldType.Name + "_" + info.Name;
            SaveValue(saveKeyName, info.GetValue(data));
        }
        PlayerPrefs.Save();
    }
    private void SaveValue(string keyName, object value)
    {
        Type fieldType = value.GetType();
        if (fieldType == typeof(int))
        {
            PlayerPrefs.SetInt(keyName, (int)value);
        }
        else if (fieldType == typeof(float))
        {
            PlayerPrefs.SetFloat(keyName, (float)value);
        }
        else if (fieldType == typeof(string))
        {
            PlayerPrefs.SetString(keyName, value.ToString());
        }
        else if (fieldType == typeof(bool))
        {
            PlayerPrefs.SetInt(keyName, (bool)value ? 1 : 0);
        }
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            IList list = value as IList;
            PlayerPrefs.SetInt(keyName, list.Count);
            int index = 0;
            foreach (object obj in list)
            {
                SaveValue(keyName + index, obj);
                ++index;
            }
        }
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            IDictionary dic = value as IDictionary;
            PlayerPrefs.SetInt(keyName, dic.Count);
            int index = 0;
            foreach (object key in dic.Keys)
            {
                SaveValue(keyName + "_key_" + index, key);
                SaveValue(keyName + "_value_" + index, dic[key]);
                ++index;
            }
        }
        else
        {
            SaveData(keyName, value);
        }
    }
    public object LoadData(string keyName, Type type)
    {
        object data = Activator.CreateInstance(type);
        FieldInfo[] infos = type.GetFields();
        string loadKeyName;
        FieldInfo info;
        for (int i = 0; i < infos.Length; i++)
        {
            info = infos[i];
            loadKeyName = keyName + "_" + type.Name +
                "_" + info.FieldType.Name + "_" + info.Name;
            info.SetValue(data, LoadValue(loadKeyName, info.FieldType));
        }
        return data;
    }
    private object LoadValue(string keyName, Type fieldType)
    {

        if (fieldType == typeof(int))
        {
            return PlayerPrefs.GetInt(keyName, 0);
        }
        else if (fieldType == typeof(float))
        {
            return PlayerPrefs.GetFloat(keyName, 0);
        }
        else if (fieldType == typeof(string))
        {
            return PlayerPrefs.GetString(keyName, "");
        }
        else if (fieldType == typeof(bool))
        {
            return PlayerPrefs.GetInt(keyName, 0) == 1 ? true : false;
        }
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            int count = PlayerPrefs.GetInt(keyName, 0);
            IList list = Activator.CreateInstance(fieldType) as IList;
            for (int i = 0; i < count; i++)
            {
                list.Add(LoadValue(keyName + i, fieldType.GetGenericArguments()[0]));
            }
            return list;
        }
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            int count = PlayerPrefs.GetInt(keyName, 0);
            IDictionary dic = Activator.CreateInstance(fieldType) as IDictionary;
            Type[] kvType = fieldType.GetGenericArguments();
            for (int i = 0; i < count; i++)
            {
                dic.Add(LoadValue(keyName + "_key_" + i, kvType[0]),
                         LoadValue(keyName + "_value_" + i, kvType[1]));
            }
            return dic;
        }
        else
        {
            return LoadData(keyName, fieldType);
        }

    }
}
