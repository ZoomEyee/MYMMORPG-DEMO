using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class XmlDataMgr : Singleton<XmlDataMgr>
{
    public string XML_PER_PATH = Application.persistentDataPath + "/Xml/";
    public string XML_STR_PATH = Application.streamingAssetsPath + "/ExcelData/Xml/";
    public void SaveData<T>(string fileName, T data, bool isStreaming = false) where T : class
    {
        string path = isStreaming ? XML_STR_PATH + fileName + ".xml" : XML_PER_PATH + fileName + ".xml";
        if (!File.Exists(XML_PER_PATH) || !File.Exists(XML_STR_PATH))
        {
            Directory.CreateDirectory(XML_PER_PATH);
            Directory.CreateDirectory(XML_STR_PATH);
        }
        using (StreamWriter writer = new StreamWriter(path))
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            s.Serialize(writer, data);
        }
    }

    public T LoadData<T>(string fileName, bool isStreaming = false) where T : class
    {
        string path = isStreaming ? XML_STR_PATH + fileName + ".xml" : XML_PER_PATH + fileName + ".xml";
        if (!File.Exists(path))
            return Activator.CreateInstance(typeof(T)) as T;

        using (StreamReader reader = new StreamReader(path))
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            return s.Deserialize(reader) as T;
        }
    }

}
