using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GUI;

public class CustomGUIToolbar : CustomGUIControl
{
    public GUIContent[] allContents;
    public List<UnityAction> clickEvents;

    public int selected = 0;
    private int oldSelected = 0;


    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        selected = GUI.Toolbar(guiPos.Pos, selected, allContents);
        if (oldSelected != selected)
        {
            clickEvents?[selected]?.Invoke();
            oldSelected = selected;
        }
    }
    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        selected = GUI.Toolbar(guiPos.Pos, selected, allContents, style);
        if (oldSelected != selected)
        {
            clickEvents?[selected]?.Invoke();
            oldSelected = selected;
        }
    }
}
