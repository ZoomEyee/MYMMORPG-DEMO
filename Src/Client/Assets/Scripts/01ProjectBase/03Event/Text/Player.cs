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
        Debug.Log("����յ���������");
    }
    void MonsterDeadLog0()
    {
        Debug.Log("����յ���������0");
    }
    void MonsterDeadAward(Monster monster)
    {
        Debug.LogFormat("��{0},��һ��{1}���,{2}����ֵ", monster.monsterName, monster.goldCoin, monster.exp);
    }
}
