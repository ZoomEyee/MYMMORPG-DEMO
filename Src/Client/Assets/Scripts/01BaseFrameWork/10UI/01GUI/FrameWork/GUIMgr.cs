using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GUIMgr : Singleton<GUIMgr>
{
    private Dictionary<string, CustomGUIBasePanel> panelDic = new Dictionary<string, CustomGUIBasePanel>();
    private GameObject gUIRoot = new GameObject("GUIRoot");
    public string panelPath = "GUI/Panel/";
    public void ShowPanel<T>(UnityAction<T> callBack = null) where T : CustomGUIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
            return;
        ResourcesMgr.Instance.LoadAsync<GameObject>(panelPath + panelName, (panelObj) =>
        {
            panelObj.transform.SetParent(gUIRoot.transform);
            panelObj.transform.localPosition = Vector3.zero;
            panelObj.transform.localScale = Vector3.one;

            T panel = panelObj.GetComponent<T>();
            panelDic.Add(panelName, panel);
            panel.ShowMe();
            if (callBack != null)
                callBack(panel);
        });
    }
    public void HidePanel<T>(bool isDel = false) where T : CustomGUIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            if (isDel)
            {
                GameObject.Destroy(panelDic[panelName].gameObject);
                panelDic.Remove(panelName);
            }
            else
                panelDic[panelName].HideMe();
        }
    }
    public T GetPanel<T>() where T : CustomGUIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
            return panelDic[panelName] as T;
        return null;
    }
    public T GetControl<T>(string panelName, string controlName) where T : CustomGUIControl
    {
        return panelDic?[panelName]?.GetControl<T>(controlName);      
    }

}
