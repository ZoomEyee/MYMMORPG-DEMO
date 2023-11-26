using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEventInfo
{

}
public class EventInfo : IEventInfo
{
    public UnityAction actions;
    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}
public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;
    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}


public class EventCenter : Singleton<EventCenter>
{
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    public void AddEventListener(string EventName, UnityAction action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventInfo)
            (eventDic[EventName] as EventInfo).actions += action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��AddʱEventName�Ѿ���EventInfo<T>ռ��");
        else
            eventDic.Add(EventName, new EventInfo(action));
    }
    public void AddEventListener<T>(string EventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventInfo<T>)
            (eventDic[EventName] as EventInfo<T>).actions += action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��AddʱEventName�Ѿ���EventInfoռ��");
        else
            eventDic.Add(EventName, new EventInfo<T>(action));
    }
    public void RemoveEventListener(string EventName, UnityAction action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventInfo)
            (eventDic[EventName] as EventInfo).actions -= action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��RemoveʱEventName�Ѿ���EventInfo<T>ռ��");
    }
    public void RemoveEventListener<T>(string EventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(EventName) && eventDic[EventName] is EventInfo<T>)
            (eventDic[EventName] as EventInfo<T>).actions -= action;
        else if (eventDic.ContainsKey(EventName))
            Debug.LogError("ִ��RemoveʱEventName�Ѿ���EventInfoռ��");
    }
    public void EventTrigger(string EventName)
    {
        if (eventDic.ContainsKey(EventName))
            (eventDic[EventName] as EventInfo).actions?.Invoke();
    }
    public void EventTrigger<T>(string EventName, T info)
    {
        if (eventDic.ContainsKey(EventName))
            (eventDic[EventName] as EventInfo<T>).actions?.Invoke(info);
    }

    public void CLear()
    {
        eventDic.Clear();
    }
}
