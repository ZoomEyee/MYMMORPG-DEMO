using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomGUIBasePanel : MonoBehaviour
{
    protected CustomGUIControl[] controls;
    protected Dictionary<string, CustomGUIControl> controlDic = new Dictionary<string, CustomGUIControl>();
    public GUISkin guiSkin;
    protected virtual void Awake()
    {
        FindChildrenControl();
    }
    protected virtual void Start()
    {
        Init();
    }    
    public virtual void Init() { }
    public virtual void ShowMe()
    {
        this.gameObject.SetActive(true);
    }
    public virtual void HideMe(UnityAction callBack = null)
    {
        this.gameObject.SetActive(false);
        if (callBack != null)
            callBack();
    }
    protected virtual void OnGUI() { }
    public T GetControl<T>(string controlName) where T : CustomGUIControl
    {
        if (controlDic.ContainsKey(controlName))
            return controlDic[controlName] as T;
        return null;
    }
    protected void FindChildrenControl()
    {
        controls = this.GetComponentsInChildren<CustomGUIControl>();
        if (controls == null)
            return;
        for (int i = 0; i < controls.Length; ++i)
        {
            string controlName = controls[i].gameObject.name;
            if (!controlDic.ContainsKey(controlName))
                controlDic.Add(controlName, controls[i]);
        }
    }
}
