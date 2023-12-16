using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomGUIControl : MonoBehaviour
{
    public CustomGUIPos guiPos;
    public GUIContent content;
    public bool isStyle = false;
    public GUIStyle style;
    
    
    public void DrawGUI(Rect groupPos)
    {
        if(isStyle)
            StyleOnDraw(groupPos);
        else
            StyleOffDraw(groupPos);
    }
    protected abstract void StyleOnDraw(Rect groupPos);
    protected abstract void StyleOffDraw(Rect groupPos);
}
