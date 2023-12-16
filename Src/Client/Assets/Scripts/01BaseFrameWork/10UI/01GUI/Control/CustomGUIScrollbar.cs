using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomGUIScrollbar : CustomGUIControl
{
    public float minValue = 0;
    public float maxValue = 1;
    public float nowValue = 0;
    public float sizeValue = 0;
    public E_Direction type = E_Direction.Horizontal;
    public GUIStyle styleThumb;
    public event UnityAction<float> changeValue;

    private float oldValue = 0;

    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        switch (type)
        {
            case E_Direction.Horizontal:
                nowValue = GUI.HorizontalScrollbar(guiPos.Pos, nowValue, sizeValue,minValue, maxValue);
                break;
            case E_Direction.Vertical:
                nowValue = GUI.VerticalScrollbar(guiPos.Pos, nowValue, sizeValue, minValue, maxValue);
                break;
        }

        if(oldValue != nowValue)
        {
            changeValue?.Invoke(nowValue);
            oldValue = nowValue;
        }
        
    }

    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        switch (type)
        {
            case E_Direction.Horizontal:
                nowValue = GUI.HorizontalScrollbar(guiPos.Pos, nowValue, sizeValue, minValue, maxValue, style);
                break;
            case E_Direction.Vertical:
                nowValue = GUI.VerticalScrollbar(guiPos.Pos, nowValue, sizeValue, minValue, maxValue, style);
                break;
        }

        if (oldValue != nowValue)
        {
            changeValue?.Invoke(nowValue);
            oldValue = nowValue;
        }
    }
}
