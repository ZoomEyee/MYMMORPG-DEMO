using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class TcpNetMgr : MonoSingleton<TcpNetMgr>
{
    public int SEND_HEART_MSG_TIME = 2;
    private Socket clientSocket;
    private Queue<BaseMsg> sendMsgQueue = new Queue<BaseMsg>();
    private Queue<BaseMsg> receiveMsgQueue = new Queue<BaseMsg>();
    private byte[] cacheBytes = new byte[1024 * 1024];
    private int cacheNum = 0;
    private bool isConnected = false;
    private HeartMsg hearMsg = new HeartMsg();
    public void Send(BaseMsg msg)
    {
        sendMsgQueue.Enqueue(msg);
    }
    public void SendBytes(byte[] bytes)
    {
        clientSocket.Send(bytes);
    }
    private void SendHeartMsg()
    {
        if (isConnected)
            Send(hearMsg);
    }
    public void Connect(string ip, int port)
    {
        if (isConnected)
            return;
        if (clientSocket == null)
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        try
        {
            clientSocket.Connect(ipPoint);
            isConnected = true;
            ThreadPool.QueueUserWorkItem(SendMsg);
            ThreadPool.QueueUserWorkItem(ReceiveMsg);
        }
        catch (SocketException e)
        {
            if (e.ErrorCode == 10061)
                print("服务器拒绝连接");
            else
                print("连接失败" + e.ErrorCode + e.Message);
        }
    }
    private void SendMsg(object obj)
    {
        while (isConnected)
        {
            if (sendMsgQueue.Count > 0)
            {
                clientSocket.Send(sendMsgQueue.Dequeue().Writing());
            }
        }
    }
    private void ReceiveMsg(object obj)
    {
        while (isConnected)
        {
            if (clientSocket.Available > 0)
            {
                byte[] receiveBytes = new byte[1024 * 1024];
                int receiveNum = clientSocket.Receive(receiveBytes);
                HandleReceiveMsg(receiveBytes, receiveNum);
            }
        }
    }
    private void HandleReceiveMsg(byte[] receiveBytes, int receiveNum)
    {
        int msgID = 0;
        int msgLength = 0;
        int nowIndex = 0;
        receiveBytes.CopyTo(cacheBytes, cacheNum);
        cacheNum += receiveNum;

        while (true)
        {
            msgLength = -1;
            if (cacheNum - nowIndex >= 8)
            {
                msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
                msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
            }
            if (cacheNum - nowIndex >= msgLength && msgLength != -1)
            {
                BaseMsg baseMsg = null;
                switch (msgID)
                {
                    case 1001:
                        PlayerMsg msg = new PlayerMsg();
                        msg.Reading(cacheBytes, nowIndex);
                        baseMsg = msg;
                        break;
                }
                if (baseMsg != null)
                    receiveMsgQueue.Enqueue(baseMsg);
                nowIndex += msgLength;
                if (nowIndex == cacheNum)
                {
                    cacheNum = 0;
                    break;
                }
            }
            else
            {
                if (msgLength != -1)
                    nowIndex -= 8;
                Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                cacheNum = cacheNum - nowIndex;
                break;
            }
        }

    }
    public void Close()
    {
        if (clientSocket != null)
        {
            print("客户端主动断开连接");
            QuitMsg msg = new QuitMsg();
            clientSocket.Send(msg.Writing());
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Disconnect(false);
            clientSocket.Close();
            clientSocket = null;
            isConnected = false;
        }
    }
    void Awake()
    {
        InvokeRepeating("SendHeartMsg", 0, SEND_HEART_MSG_TIME);
    }
    void Update()
    {
        if (receiveMsgQueue.Count > 0)
        {
            BaseMsg msg = receiveMsgQueue.Dequeue();
            if (msg is PlayerMsg)
            {
                PlayerMsg playerMsg = (msg as PlayerMsg);
                print(playerMsg.playerID);
                print(playerMsg.playerData.name);
                print(playerMsg.playerData.lev);
                print(playerMsg.playerData.atk);
            }
        }
    }
    private void OnDestroy()
    {
        Close();
    }
}
