using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Text
{
    public int i = 5;
    public float f = 6.3f;
    public string s = "Hello World!!";
    public HashSet<int> l;
    public XmlSerizlizerDictionary<string, string> d = new XmlSerizlizerDictionary<string, string>();
    //public Dictionary<string, string> d = new Dictionary<string, string>();

    public Text()
    {
        d.Add("a", "���");
        d.Add("b", "лл");
        d.Add("c", "�Բ���");
        l = new HashSet<int> { 1, 2, 3 };
    }
}
public struct Textstruct
{
    public int ii;
    public float ff;
    public string ss;
    public Textstruct(int i = 0)
    {
        ii = 9;
        ff = 3.14f;
        ss = "�Ұ���";
    }
}
public class PrefsText : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))//��������� ����
        {
            Text text01 = new Text();
            text01.i = 9;
            PlayerPrefsDataMgr.Instance.SaveData("PrefsText", text01);
            Text text02 = PlayerPrefsDataMgr.Instance.LoadData("PrefsText", typeof(Text)) as Text;
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
        if (Input.GetMouseButtonDown(1))//Excel��������� ����
        {
            PlayerInfoContainer playerInfoContainer = PlayerPrefsDataMgr.Instance.LoadData("PlayerInfoContainer",typeof(PlayerInfoContainer)) as PlayerInfoContainer;
            XmlSerizlizerDictionary<int, PlayerInfo> XmlSerizlizerDictionary = playerInfoContainer.dataDic;
            foreach (KeyValuePair<int, PlayerInfo> item in XmlSerizlizerDictionary)
            {
                Debug.Log(item.Key);
                Debug.Log(item.Value.atk); 
            }
        }
        if (Input.GetMouseButtonDown(2))//�ṹ����� ����
        {
            Textstruct text1 = new Textstruct(0);
            text1.ii = 88;
            PlayerPrefsDataMgr.Instance.SaveData("PrefsTextstruct", text1);
            Textstruct text2 = (Textstruct)PlayerPrefsDataMgr.Instance.LoadData("PrefsTextstruct", typeof(Textstruct));
            Debug.Log(text2.ii);
            Debug.Log(text2.ff);
            Debug.Log(text2.ss);
        }

    }
}
