using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskTest : MonoBehaviour
{
    void Start()
    {
        EventCenter.Instance.AddEventListener("MonsterDeadLog", MonsterDeadLog);
        EventCenter.Instance.AddEventListener<Monster>("MonsterDeadAward", MonsterDeadAward);
    }

    void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener("MonsterDeadLog", MonsterDeadLog);
        EventCenter.Instance.RemoveEventListener<Monster>("MonsterDeadAward", MonsterDeadAward);
    }

    void MonsterDeadLog()
    {
        Debug.Log("任务收到怪物死了");
    }
    void MonsterDeadAward(Monster monster)
    {
        Debug.LogFormat("达成成就击败{0}", monster.monsterName);
    }
}
