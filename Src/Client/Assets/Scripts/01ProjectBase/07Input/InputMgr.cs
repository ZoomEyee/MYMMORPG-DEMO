using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMgr : Singleton<InputMgr>
{
    public bool isStart;
    public InputMgr()
    {
        PublicMonoMgr.Instance.AddUpdateListener(InputUpdate);
    }

    private void CheckKeyCode(KeyCode key)
    {
        if (Input.GetKeyDown(key))
            EventCenter.Instance.EventTrigger("���̰���", key);
        if (Input.GetKeyUp(key))
            EventCenter.Instance.EventTrigger("���̰���", key);
    }

    private void InputUpdate()
    {
        if (!isStart)
            return;
        CheckKeyCode(KeyCode.W);
        CheckKeyCode(KeyCode.S);
        CheckKeyCode(KeyCode.A);
        CheckKeyCode(KeyCode.D);
    }
}
