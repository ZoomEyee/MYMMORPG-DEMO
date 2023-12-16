using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class CustomGUIPanel : CustomGUIBasePanel
{
    private Rect groupRect = new Rect(0, 0, Screen.width, Screen.height);
    protected override void OnGUI()
    {
        if (!Application.isPlaying)
            FindChildrenControl();
        groupRect.width = Screen.width;
        groupRect.height = Screen.height;
        if (guiSkin != null)
            GUI.skin = guiSkin;
        for (int i = 0; i < controls.Length; i++)
        {
            controls[i].DrawGUI(groupRect);
        }
    }
}
