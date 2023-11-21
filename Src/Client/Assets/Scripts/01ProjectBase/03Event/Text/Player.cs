using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    void Start()
    {
        EventCenter.Instance.AddEventListener("MonsterDeadLog", MonsterDeadLog);
        EventCenter.Instance.AddEventListener("MonsterDeadLog", MonsterDeadLog0);
        EventCenter.Instance.AddEventListener<Monster>("MonsterDeadAward", MonsterDeadAward);
    }

    void OnDestroy()
    {
        EventCenter.Instance.RemoveEventListener("MonsterDeadLog", MonsterDeadLog);
        EventCenter.Instance.RemoveEventListener<Monster>("MonsterDeadAward", MonsterDeadAward);
    }

    void MonsterDeadLog()
    {
        Debug.Log("玩家收到怪物死了");
    }
    void MonsterDeadLog0()
    {
        Debug.Log("玩家收到怪物死了0");
    }
    void MonsterDeadAward(Monster monster)
    {
        Debug.LogFormat("是{0},玩家获得{1}金币,{2}经验值", monster.monsterName, monster.goldCoin, monster.exp);
    }
}
