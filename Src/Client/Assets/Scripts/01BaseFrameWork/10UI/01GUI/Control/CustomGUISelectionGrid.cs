using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GUI;

public class CustomGUISelectionGrid : CustomGUIControl
{
    public GUIContent[] allContents;
    public List<UnityAction> clickEvents;
    public int xCount = 0;
    public int selected = 0;
    private int oldSelected = 0;


    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        selected = GUI.SelectionGrid(guiPos.Pos, selected, allContents, xCount);
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
        selected = GUI.SelectionGrid(guiPos.Pos, selected, allContents, xCount, style);
        if (oldSelected != selected)
        {
            clickEvents?[selected]?.Invoke();
            oldSelected = selected;
        }
    }
}
