using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUILoginPanelTest : CustomGUIPanel
{
    public override void Init()
    {
        GetControl<CustomGUIButton>("LoginButton").clickEvent += () =>
        {
            Debug.Log("µÇÂ¼"); GUIMgr.Instance.HidePanel<GUILoginPanelTest>();
        };
        GUIMgr.Instance.GetControl<CustomGUIButton>("GUILoginPanelTest", "RegisterButton").clickEvent+= () =>
        {
            Debug.Log("×¢²á"); GUIMgr.Instance.HidePanel<GUILoginPanelTest>();
        };
    }
}
