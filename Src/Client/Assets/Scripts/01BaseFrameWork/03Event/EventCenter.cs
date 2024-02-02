using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEventCustomInfo
{

}
public class EventCustomInfo : IEventCustomInfo
{
    public UnityAction actions;
    public EventCustomInfo(UnityAction action)
    {
        actions += action;
    }
}
public class EventCustomInfo<T> : IEventCustomInfo
{
    public UnityAction<T> actions;
    public EventCustomInfo(UnityAction<T> action)
    {
        actions += action;
    }
}


public class EventCenter : Singleton<EventCenter>
{
    private Dictionary<string, IEventCustomInfo> eventDic = new Dictionary<string, IEventCustomInfo>();

    public void AddEventListener(string EventName, UnityAction action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventCustomInfo)
            (eventDic[EventName] as EventCustomInfo).actions += action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��AddʱEventName�Ѿ���EventInfo<T>ռ��");
        else
            eventDic.Add(EventName, new EventCustomInfo(action));
    }
    public void AddEventListener<T>(string EventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventCustomInfo<T>)
            (eventDic[EventName] as EventCustomInfo<T>).actions += action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��AddʱEventName�Ѿ���EventInfoռ��");
        else
            eventDic.Add(EventName, new EventCustomInfo<T>(action));
    }
    public void RemoveEventListener(string EventName, UnityAction action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventCustomInfo)
            (eventDic[EventName] as EventCustomInfo).actions -= action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��RemoveʱEventName�Ѿ���EventInfo<T>ռ��");
    }
    public void RemoveEventListener<T>(string EventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventCustomInfo<T>)
            (eventDic[EventName] as EventCustomInfo<T>).actions -= action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��RemoveʱEventName�Ѿ���EventInfoռ��");
    }
    public void EventTrigger(string EventName)
    {
        if (eventDic.ContainsKey(EventName))
            (eventDic[EventName] as EventCustomInfo).actions?.Invoke();
    }
    public void EventTrigger<T>(string EventName, T info)
    {
        if (eventDic.ContainsKey(EventName))
            (eventDic[EventName] as EventCustomInfo<T>).actions?.Invoke(info);
    }

    public void CLear()
    {
        eventDic.Clear();
    }
}
