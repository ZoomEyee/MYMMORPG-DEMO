using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIMainTest : MonoBehaviour
{
    void Start()
    {
        GUIMgr.Instance.ShowPanel<GUILoginPanelTest>((loginPanel) => { Debug.Log("��ʾ�ɹ�" + loginPanel.ToString()); });
    }

}
