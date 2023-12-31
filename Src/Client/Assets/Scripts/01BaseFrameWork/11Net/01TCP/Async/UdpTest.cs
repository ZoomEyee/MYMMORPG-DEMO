using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UdpTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TcpAsyncNetMgr.Instance.Connect("127.0.0.1", 8080);
            Debug.Log("Á´½Ó³É¹¦");
        }
        if (Input.GetMouseButtonDown(1))
        {
            PlayerMsg playerMsg = new PlayerMsg();
            PlayerData playerData = new PlayerData();
            playerData.atk = 999;
            playerData.lev = 100;
            playerData.name = "CZK";
            playerMsg.playerData = playerData;
            TcpAsyncNetMgr.Instance.Send(playerMsg);
        }
    }
}
