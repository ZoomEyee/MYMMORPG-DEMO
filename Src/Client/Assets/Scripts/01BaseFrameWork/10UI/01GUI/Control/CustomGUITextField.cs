using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomGUITextField : CustomGUIControl
{
    public event UnityAction<string> textChange;
    private string oldStr = "";
    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        content.text = GUI.TextField(guiPos.Pos, content.text);
        if(oldStr != content.text)
        {
            textChange?.Invoke(oldStr);
            oldStr = content.text;
        }
    }
    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        content.text = GUI.TextField(guiPos.Pos, content.text, style);
        if (oldStr != content.text)
        {
            textChange?.Invoke(oldStr);
            oldStr = content.text;
        }
    }
}
