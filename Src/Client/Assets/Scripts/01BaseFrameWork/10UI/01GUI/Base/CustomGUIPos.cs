using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_Alignment_Type
{
    Up,
    Down,
    Left,
    Right,
    Center,
    Left_Up,
    Left_Down,
    Right_Up,
    Right_Down,
}
[System.Serializable]
public class CustomGUIPos
{
    private Rect rPos = new Rect(0, 0, 0, 0);
    public E_Alignment_Type screen_Alignment_Type = E_Alignment_Type.Center;
    public E_Alignment_Type control_Center_Alignment_Type = E_Alignment_Type.Center;
    public float width = 100;
    public float height = 50;
    public Vector2 offsetPos;
    private Vector2 centerPos;

    [HideInInspector]
    public Rect rootRect;

    private void CalcCenterPos()
    {
        switch (control_Center_Alignment_Type)
        {
            case E_Alignment_Type.Up:
                centerPos.x = -width / 2;
                centerPos.y = 0;
                break;
            case E_Alignment_Type.Down:
                centerPos.x = -width / 2;
                centerPos.y = -height;
                break;
            case E_Alignment_Type.Left:
                centerPos.x = 0;
                centerPos.y = -height / 2;
                break;
            case E_Alignment_Type.Right:
                centerPos.x = -width;
                centerPos.y = -height / 2;
                break;
            case E_Alignment_Type.Center:
                centerPos.x = -width / 2;
                centerPos.y = -height / 2;
                break;
            case E_Alignment_Type.Left_Up:
                centerPos.x = 0;
                centerPos.y = 0;
                break;
            case E_Alignment_Type.Left_Down:
                centerPos.x = 0;
                centerPos.y = -height;
                break;
            case E_Alignment_Type.Right_Up:
                centerPos.x = -width;
                centerPos.y = 0;
                break;
            case E_Alignment_Type.Right_Down:
                centerPos.x = -width;
                centerPos.y = -height;
                break;
        }
    }
    private void CalcRootos()
    {
        switch (screen_Alignment_Type)
        {
            case E_Alignment_Type.Up:
                rPos.x = rootRect.x + rootRect.width / 2 + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + centerPos.y + offsetPos.y;
                break;
            case E_Alignment_Type.Down:
                rPos.x = rootRect.x + rootRect.width / 2 + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + rootRect.height + centerPos.y - offsetPos.y;
                break;
            case E_Alignment_Type.Left:
                rPos.x = rootRect.x + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + rootRect.height / 2 + centerPos.y - offsetPos.y;
                break;
            case E_Alignment_Type.Right:
                rPos.x = rootRect.x + rootRect.width + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + rootRect.height / 2 + centerPos.y - offsetPos.y;
                break;
            case E_Alignment_Type.Center:
                rPos.x = rootRect.x + rootRect.width / 2 + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + rootRect.height / 2 + centerPos.y - offsetPos.y;
                break;
            case E_Alignment_Type.Left_Up:
                rPos.x = rootRect.x + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + centerPos.y - offsetPos.y;
                break;
            case E_Alignment_Type.Left_Down:
                rPos.x = rootRect.x + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + rootRect.height + centerPos.y - offsetPos.y;
                break;
            case E_Alignment_Type.Right_Up:
                rPos.x = rootRect.x + rootRect.width + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + centerPos.y + offsetPos.y;
                break;
            case E_Alignment_Type.Right_Down:
                rPos.x = rootRect.x + rootRect.width + centerPos.x + offsetPos.x;
                rPos.y = rootRect.y + rootRect.height + centerPos.y - offsetPos.y;
                break;
        }
    }
    public Rect Pos
    {
        get
        {
            CalcCenterPos();
            CalcRootos();
            rPos.width = width;
            rPos.height = height;
            return rPos;
        }
    }

}
