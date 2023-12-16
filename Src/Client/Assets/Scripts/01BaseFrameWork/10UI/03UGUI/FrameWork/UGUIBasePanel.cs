using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UGUIBasePanel : MonoBehaviour
{
    public float alphaSpeed = 10;
    private bool isShow;
    private UnityAction hideCallBack;
    private CanvasGroup canvasGroup;
    public Dictionary<string, List<UIBehaviour>> controlDic = new Dictionary<string, List<UIBehaviour>>();
    protected virtual void Awake()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        FindChildrenControl();
    }
    protected virtual void Start()
    {
        Init();
    }
    public abstract void Init();
    public virtual void ShowMe()
    {
        isShow = true;
        canvasGroup.alpha = 0;
    }
    public virtual void HideMe(UnityAction callBack = null)
    {
        isShow = false;
        canvasGroup.alpha = 1;
        if (callBack != null)
            hideCallBack = callBack;
    }
    protected virtual void Update()
    {
        if (isShow && canvasGroup.alpha != 1)
        {
            canvasGroup.alpha += alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha >= 1)
                canvasGroup.alpha = 1;
        }
        else if (!isShow)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                hideCallBack?.Invoke();
            }
        }
    }
    public T GetControl<T>(string controlName, string gameObjName = null) where T : UIBehaviour
    {
        if (gameObjName == null)
        {
            if (controlDic.ContainsKey(controlName))
            {
                for (int i = 0; i < controlDic[controlName].Count; ++i)
                {
                    if (controlDic[controlName][i] is T)
                        return controlDic[controlName][i] as T;
                }
            }
        }
        else
        {
            if (controlDic.ContainsKey(controlName))
            {
                for (int i = 0; i < controlDic[controlName].Count; ++i)
                {
                    if (controlDic[controlName][i] is T && controlDic[controlName][i].gameObject.name == gameObjName)
                        return controlDic[controlName][i] as T;
                }
            }
        }
        return null;
    }
    private void FindChildrenControl()
    {
        UIBehaviour[] controls = this.GetComponentsInChildren<UIBehaviour>();
        if (controls == null)
            return;
        for (int i = 0; i < controls.Length; ++i)
        {
            string controlName;
            if (controls[i].transform.parent == null || controls[i].transform.parent.name == controls[i].transform.root.name)
                controlName = controls[i].gameObject.name;
            else
            {
                Transform currentTransform = controls[i].transform;
                while (currentTransform.parent.parent != null)
                {
                    currentTransform = currentTransform.parent;
                }
                controlName = currentTransform.gameObject.name;
            }

            if (controlDic.ContainsKey(controlName))
                controlDic[controlName].Add(controls[i]);
            else
                controlDic.Add(controlName, new List<UIBehaviour>() { controls[i] });
        }
    }
}
