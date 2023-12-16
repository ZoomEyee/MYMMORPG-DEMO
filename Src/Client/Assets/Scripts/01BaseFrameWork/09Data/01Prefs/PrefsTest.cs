using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Test
{
    public int i = 5;
    public float f = 6.3f;
    public string s = "Hello World!!";
    public HashSet<int> l;
    public XmlSerizlizerDictionary<string, string> d = new XmlSerizlizerDictionary<string, string>();
    //public Dictionary<string, string> d = new Dictionary<string, string>();

    public Test()
    {
        d.Add("a", "你好");
        d.Add("b", "谢谢");
        d.Add("c", "对不起");
        l = new HashSet<int> { 1, 2, 3 };
    }
}
public struct Teststruct
{
    public int ii;
    public float ff;
    public string ss;
    public Teststruct(int i = 0)
    {
        ii = 9;
        ff = 3.14f;
        ss = "我爱你";
    }
}
public class PrefsTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))//正常类测试 正常
        {
            Test text01 = new Test();
            text01.i = 9;
            PlayerPrefsDataMgr.Instance.SaveData("PrefsText", text01);
            Test text02 = PlayerPrefsDataMgr.Instance.LoadData("PrefsText", typeof(Test)) as Test;
            Debug.Log(text02.i);
            Debug.Log(text02.f);
            Debug.Log(text02.s);
            foreach (var item in text02.l)
            {
                Debug.Log(item);
            }
            foreach (KeyValuePair<string, string> item in text02.d)
            {
                Debug.Log(item.Key);
                Debug.Log(item.Value);
            }
        }
        if (Input.GetMouseButtonDown(1))//Excel生产类测试 正常
        {
            PlayerInfoContainer playerInfoContainer = PlayerPrefsDataMgr.Instance.LoadData("PlayerInfoContainer",typeof(PlayerInfoContainer)) as PlayerInfoContainer;
            XmlSerizlizerDictionary<int, PlayerInfo> XmlSerizlizerDictionary = playerInfoContainer.dataDic;
            foreach (KeyValuePair<int, PlayerInfo> item in XmlSerizlizerDictionary)
            {
                Debug.Log(item.Key);
                Debug.Log(item.Value.atk); 
            }
        }
        if (Input.GetMouseButtonDown(2))//结构体测试 正常
        {
            Teststruct text1 = new Teststruct(0);
            text1.ii = 88;
            PlayerPrefsDataMgr.Instance.SaveData("PrefsTextstruct", text1);
            Teststruct text2 = (Teststruct)PlayerPrefsDataMgr.Instance.LoadData("PrefsTextstruct", typeof(Teststruct));
            Debug.Log(text2.ii);
            Debug.Log(text2.ff);
            Debug.Log(text2.ss);
        }

    }
}
