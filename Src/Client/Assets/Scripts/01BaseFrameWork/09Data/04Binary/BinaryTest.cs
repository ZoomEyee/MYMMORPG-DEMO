using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TestBinary
{
    public int i = 5;
    public float f = 6.3f;
    public string s = "Hello World!!";
    public List<int> l;
    public Dictionary<string, string> d = new Dictionary<string, string>();
    public TestB textB = new TestB();
    //public TTDictionary<int, string> tdic = new TTDictionary<int, string>();

    public TestBinary()
    {
        d.Add("a", "你好");
        d.Add("b", "谢谢");
        d.Add("c", "对不起");
        l = new List<int> { 1, 2, 3 };
        //tdic.Add(1, "我爱你");
    }
}
[Serializable]
public class TestB
{
    public int i = 1;
    public float f = 2f;
}
[Serializable]
public class TTDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    
}
public class BinaryTest : MonoBehaviour
{

    void Update()
    {
        if (Input.GetMouseButtonDown(0))//正常类测试 正常
        {
            TestBinary text01 = new TestBinary();
            text01.i = 9;
            BinaryDataMgr.Instance.SaveData("BinaryText", text01);
            TestBinary text02 = BinaryDataMgr.Instance.LoadData<TestBinary>("BinaryText");
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
            Debug.Log(text02.textB.i);
            Debug.Log(text02.textB.f);
            //Debug.Log(text02.tdic[1]);

        }
        if (Input.GetMouseButtonDown(1))//Excel生产类测试 正常
        {
            PlayerInfoContainer playerInfoContainer = BinaryDataMgr.Instance.LoadTable<PlayerInfo, PlayerInfoContainer>();
            XmlSerizlizerDictionary<int, PlayerInfo> XmlSerizlizerDictionary = playerInfoContainer.dataDic;
            foreach (KeyValuePair<int, PlayerInfo> item in XmlSerizlizerDictionary)
            {
                Debug.Log(item.Key);
                Debug.Log(item.Value.atk);
            }
        }
    }
}
