using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGUITexture : CustomGUIControl
{
    public ScaleMode scaleMode = ScaleMode.StretchToFill;
    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        GUI.DrawTexture(guiPos.Pos, content.image, scaleMode);
    }

    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        GUI.DrawTexture(guiPos.Pos, content.image, scaleMode);
    }
}
