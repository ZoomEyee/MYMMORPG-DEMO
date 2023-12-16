using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UGUIMainTest : MonoBehaviour
{
    void Start()
    {
        UGUIMgr.Instance.ShowPanel<UGUILoginPanelTest>(E_UI_Layer.Mid, (loginPanel) => { Debug.Log("œ‘ æ≥…π¶"+ loginPanel.ToString()); });
    }

}
