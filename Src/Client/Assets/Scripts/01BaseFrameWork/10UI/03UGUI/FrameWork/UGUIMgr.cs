using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum E_UI_Layer
{
    Bot,
    Mid,
    Top,
    System,
}
public class UGUIMgr : Singleton<UGUIMgr>
{
    public string canvasPath = "UGUI/Canvas";
    public string eventSystemPath = "UGUI/EventSystem";
    public string panelPath = "UGUI/";

    private RectTransform canvas;
    private Transform bot;
    private Transform mid;
    private Transform top;
    private Transform system;
    private Dictionary<string, UGUIBasePanel> panelDic = new Dictionary<string, UGUIBasePanel>();

    public UGUIMgr()
    {
        GameObject obj = ResourcesMgr.Instance.Load<GameObject>(canvasPath);
        canvas = obj.transform as RectTransform;
        GameObject.DontDestroyOnLoad(obj);

        bot = canvas.Find("Bot");
        mid = canvas.Find("Mid");
        top = canvas.Find("Top");
        system = canvas.Find("System");

        obj = ResourcesMgr.Instance.Load<GameObject>(eventSystemPath);
        GameObject.DontDestroyOnLoad(obj);
    }
    public void ShowPanel<T>(E_UI_Layer layer = E_UI_Layer.Mid, UnityAction<T> callBack = null) where T : UGUIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
            return;
        ResourcesMgr.Instance.LoadAsync<GameObject>(panelPath + panelName, (panelObj) =>
        {
            Transform father = bot;
            switch (layer)
            {
                case E_UI_Layer.Mid:
                    father = mid;
                    break;
                case E_UI_Layer.Top:
                    father = top;
                    break;
                case E_UI_Layer.System:
                    father = system;
                    break;
            }
            panelObj.transform.SetParent(father);
            panelObj.transform.localPosition = Vector3.zero;
            panelObj.transform.localScale = Vector3.one;
            (panelObj.transform as RectTransform).offsetMax = Vector2.zero;
            (panelObj.transform as RectTransform).offsetMin = Vector2.zero;

            T panel = panelObj.GetComponent<T>();
            panelDic.Add(panelName, panel);
            panel.ShowMe();
            if (callBack != null)
                callBack(panel);
        });
    }
    public void HidePanel<T>(bool isFade = true) where T : UGUIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            if (isFade)
            {
                panelDic[panelName].HideMe(() =>
                {
                    GameObject.Destroy(panelDic[panelName].gameObject);
                    panelDic.Remove(panelName);
                });
            }
            else
            {
                GameObject.Destroy(panelDic[panelName].gameObject);
                panelDic.Remove(panelName);
            }
        }
    }
    public T GetPanel<T>() where T : UGUIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
            return panelDic[panelName] as T;
        return null;
    }
    public Transform GetLayerFather(E_UI_Layer layer)
    {
        switch (layer)
        {
            case E_UI_Layer.Bot:
                return this.bot;
            case E_UI_Layer.Mid:
                return this.mid;
            case E_UI_Layer.Top:
                return this.top;
            case E_UI_Layer.System:
                return this.system;
        }
        return null;
    }
    public void AddCustomEventListener<T>(string panelName, string controlName, EventTriggerType type, UnityAction<BaseEventData> callBack) where T : UIBehaviour
    {       
        UIBehaviour control = panelDic?[panelName]?.GetControl<T>(controlName);
        if (control == null)
            return;
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callBack);
        trigger.triggers.Add(entry);
    }
}
