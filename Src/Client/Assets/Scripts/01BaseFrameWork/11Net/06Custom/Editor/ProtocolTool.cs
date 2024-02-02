using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class ProtocolTool
{
    private static string PROTO_INFO_PATH = Application.dataPath + "/Data/ProtocolXml/ProtocolInfo.xml";
    private static GenerateCSharp generateCSharp = new GenerateCSharp();

    [MenuItem("CustomTools/ProtocolTool/Generate C# script")]
    private static void GenerateCSharp()
    {
        generateCSharp.GenerateEnum(GetNodes("enum"));
        generateCSharp.GenerateData(GetNodes("data"));
        generateCSharp.GenerateMsg(GetNodes("message"));
        AssetDatabase.Refresh();
    }
    [MenuItem("CustomTools/ProtocolTool/Generate C++ script")]
    private static void GenerateC()
    {
        Debug.Log("生成C++代码,待完善");
    }
    [MenuItem("CustomTools/ProtocolTool/Generate Java script")]
    private static void GenerateJava()
    {
        Debug.Log("生成Java代码,待完善");
    }
    private static XmlNodeList GetNodes(string nodeName)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(PROTO_INFO_PATH);
        XmlNode root = xml.SelectSingleNode("messages");
        return root.SelectNodes(nodeName);
    }
}
