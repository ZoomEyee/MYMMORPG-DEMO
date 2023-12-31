using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class ProtocolTool
{
    private static string PROTO_INFO_PATH = Application.dataPath + "/Data/ProtocolXml/ProtocolInfo.xml";
    private static GenerateCSharp generateCSharp = new GenerateCSharp();

    [MenuItem("ProtocolTool/����C#�ű�")]
    private static void GenerateCSharp()
    {
        generateCSharp.GenerateEnum(GetNodes("enum"));
        generateCSharp.GenerateData(GetNodes("data"));
        generateCSharp.GenerateMsg(GetNodes("message"));
        AssetDatabase.Refresh();
    }
    [MenuItem("ProtocolTool/����C++�ű�")]
    private static void GenerateC()
    {
        Debug.Log("����C++����,������");
    }
    [MenuItem("ProtocolTool/����Java�ű�")]
    private static void GenerateJava()
    {
        Debug.Log("����Java����,������");
    }
    private static XmlNodeList GetNodes(string nodeName)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(PROTO_INFO_PATH);
        XmlNode root = xml.SelectSingleNode("messages");
        return root.SelectNodes(nodeName);
    }
}
