using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomGUIRepeatButton : CustomGUIControl
{
    public event UnityAction clickEvent;
    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        if ( GUI.RepeatButton(guiPos.Pos, content ) )
        {
            clickEvent?.Invoke();
        }
    }
    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        if (GUI.RepeatButton(guiPos.Pos, content, style))
        {
            clickEvent?.Invoke();
        }
    }
}
