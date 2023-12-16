using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XmlTest : MonoBehaviour
{   
    void Update()
    {
        if (Input.GetMouseButtonDown(0))//��������� ����
        {
            Test text01 = new Test();
            text01.i = 9;
            XmlDataMgr.Instance.SaveData("XmlText", text01);
            Test text02 = XmlDataMgr.Instance.LoadData<Test>("XmlText");
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
            PlayerInfoContainer playerInfoContainer = XmlDataMgr.Instance.LoadData<PlayerInfoContainer>("PlayerInfoContainer",true);
            XmlSerizlizerDictionary<int, PlayerInfo> XmlSerizlizerDictionary = playerInfoContainer.dataDic;
            foreach (KeyValuePair<int, PlayerInfo> item in XmlSerizlizerDictionary)
            {
                Debug.Log(item.Key);
                Debug.Log(item.Value.atk);
            }
        }
    }
}
