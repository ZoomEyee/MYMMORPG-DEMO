using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CustomGUIScrollView : CustomGUIBasePanel
{
    private Vector2 nowPos;
    public Rect allPos;
    public Rect showPos;
    public bool isStyle = false;
    public GUIStyle horizontalScrollbarStyle;
    public GUIStyle verticalScrollbarStyle;
    protected override void OnGUI()
    {
        if (!Application.isPlaying)
            FindChildrenControl();
        if (isStyle)
        {
            if (guiSkin != null)
                GUI.skin = guiSkin;
            nowPos = GUI.BeginScrollView(showPos, nowPos, allPos, horizontalScrollbarStyle, verticalScrollbarStyle);
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].DrawGUI(allPos);
            }
            GUI.EndScrollView();
        }
        else
        {
            if (guiSkin != null)
                GUI.skin = guiSkin;
            nowPos = GUI.BeginScrollView(showPos, nowPos, allPos);
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].DrawGUI(allPos);
            }
            GUI.EndScrollView();
        }
            
    }
}
