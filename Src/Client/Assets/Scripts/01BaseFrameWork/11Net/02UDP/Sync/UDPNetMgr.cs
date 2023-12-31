using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class UdpNetMgr : MonoSingleton<UdpNetMgr>
{
    private EndPoint serverIpPoint;
    private Socket clientSocket;
    private bool isClose = true;
    private Queue<BaseMsg> sendQueue = new Queue<BaseMsg>();
    private Queue<BaseMsg> receiveQueue = new Queue<BaseMsg>();
    private byte[] cacheBytes = new byte[512];
    void Update()
    {
        if(receiveQueue.Count > 0)
        {
            BaseMsg baseMsg = receiveQueue.Dequeue();
            switch (baseMsg)
            {
                case PlayerMsg msg:
                    print(msg.playerID);
                    print(msg.playerData.name);
                    print(msg.playerData.atk);
                    print(msg.playerData.lev);
                    break;
            }
        }
    }
    public void StartClient(string ip, int port)
    {
        if (!isClose)
            return;
        serverIpPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        IPEndPoint clientIpPort = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            clientSocket.Bind(clientIpPort);
            isClose = false;
            print("客户端网络启动");
            ThreadPool.QueueUserWorkItem(ReceiveMsg);
            ThreadPool.QueueUserWorkItem(SendMsg);
        }
        catch (System.Exception e)
        {
            print("启动Socket出现错误" + e.Message);
        }
    }
    private void ReceiveMsg(object obj)
    {
        EndPoint tempIpPoint = new IPEndPoint(IPAddress.Any, 0);
        int nowIndex;
        int msgID;
        int msgLength;
        while (!isClose)
        {
            if(clientSocket != null && clientSocket.Available > 0)
            {
                try
                {
                    clientSocket.ReceiveFrom(cacheBytes, ref tempIpPoint);
                    if(!tempIpPoint.Equals(serverIpPoint))
                        continue;
                    nowIndex = 0;
                    msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                    msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                    BaseMsg msg = null;
                    switch (msgID)
                    {
                        case 1001:
                            msg = new PlayerMsg();
                            msg.Reading(cacheBytes, nowIndex);
                            break;
                    }
                    if (msg != null)
                        receiveQueue.Enqueue(msg);
                }
                catch (SocketException s)
                {
                    print("接受消息出问题" + s.SocketErrorCode + s.Message);
                }
                catch (Exception e)
                {
                    print("接受消息出问题(非网络问题)" + e.Message);
                }
            }
        }
    }
    private void SendMsg(object obj)
    {
        while (!isClose)
        {
            if (clientSocket != null && sendQueue.Count > 0)
            {
                try
                {
                    clientSocket.SendTo(sendQueue.Dequeue().Writing(), serverIpPoint);
                }
                catch (SocketException s)
                {
                    print("发送消息出错" + s.SocketErrorCode + s.Message);
                }
            }
        }
    }
    public void Send(BaseMsg msg)
    {
        sendQueue.Enqueue(msg);
    }
    public void Close()
    {
        if(clientSocket != null)
        {
            isClose = true;
            QuitMsg msg = new QuitMsg();
            clientSocket.SendTo(msg.Writing(), serverIpPoint);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clientSocket = null;
        }
        
    }
    private void OnDestroy()
    {
        Close();
    }
}
