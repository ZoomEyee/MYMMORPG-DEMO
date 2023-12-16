using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGUILabel : CustomGUIControl
{
    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        GUI.Label(guiPos.Pos, content);
    }

    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        GUI.Label(guiPos.Pos, content, style);
    }
}
