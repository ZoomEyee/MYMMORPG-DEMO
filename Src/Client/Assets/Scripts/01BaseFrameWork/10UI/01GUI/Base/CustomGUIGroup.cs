using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CustomGUIGroup : CustomGUIBasePanel
{
    public Rect groupPos;
    public GUIContent content;
    public bool isStyle = false;
    public GUIStyle style;
    protected override void OnGUI()
    {
        if (!Application.isPlaying)
            FindChildrenControl();
        if (isStyle)
        {
            if (guiSkin != null)
                GUI.skin = guiSkin;
            GUI.BeginGroup(groupPos, content, style);
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].DrawGUI(groupPos);
            }
            GUI.EndGroup();
        }
        else
        {
            if (guiSkin != null)
                GUI.skin = guiSkin;
            GUI.BeginGroup(groupPos, content);
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].DrawGUI(groupPos);
            }
            GUI.EndGroup();
        }
    }
}
