using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGUIBox : CustomGUIControl
{
    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        GUI.Box(guiPos.Pos, content);
    }

    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        GUI.Box(guiPos.Pos, content, style);
    }
}
