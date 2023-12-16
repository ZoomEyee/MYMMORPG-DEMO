using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public string monsterName = "Boss";
    public int goldCoin = 500;
    public int exp = 2000;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            EventCenter.Instance.EventTrigger("MonsterDeadLog");
        }
        if (Input.GetMouseButtonDown(1))
        {
            EventCenter.Instance.EventTrigger<Monster>("MonsterDeadAward", this);
        }
    }

    void Dead()
    {
        EventCenter.Instance.EventTrigger("MonsterDeadLog");
        EventCenter.Instance.EventTrigger<Monster>("MonsterDeadAward", this);
    }
}
