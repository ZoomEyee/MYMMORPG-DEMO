using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GUI;

public class CustomGUIWindow : CustomGUIControl
{
    public WindowFunction windowEvent;
    public int windowId;
    public Rect windowPos;
    public bool isModal = false;
    protected override void StyleOffDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        if (!isModal)
            windowPos = GUI.Window(windowId, windowPos, DrawWindow, content);
        else
            GUI.ModalWindow(windowId, windowPos, DrawWindow, content);
    }
    protected override void StyleOnDraw(Rect groupPos)
    {
        if (groupPos != null)
            guiPos.rootRect = groupPos;
        if (!isModal)
            windowPos = GUI.Window(windowId, windowPos, DrawWindow, content, style);
        else
            GUI.ModalWindow(windowId, windowPos, DrawWindow, content, style);
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0, 0, windowPos.width, windowPos.height));
        if (windowEvent != null)
            windowEvent(id);
    }
}
