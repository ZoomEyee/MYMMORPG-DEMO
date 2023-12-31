using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UdpAsyncNetMgr : MonoSingleton<UdpAsyncNetMgr>
{

    private EndPoint serverIpPoint;
    private Socket clientSocket;
    private bool isClose = true;
    private Queue<BaseMsg> receiveQueue = new Queue<BaseMsg>();
    private byte[] cacheBytes = new byte[512];
    void Update()
    {
        if (receiveQueue.Count > 0)
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
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(cacheBytes, 0, cacheBytes.Length);
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            args.Completed += ReceiveMsg;
            clientSocket.ReceiveFromAsync(args);
            print("客户端网络启动");
        }
        catch (System.Exception e)
        {
            print("启动Socket出问题" + e.Message);
        }
    }
    private void ReceiveMsg(object obj, SocketAsyncEventArgs args)
    {
        int nowIndex;
        int msgID;
        int msgLength;
        if(args.SocketError == SocketError.Success)
        {
            try
            {
                if (args.RemoteEndPoint.Equals(serverIpPoint))
                {
                    nowIndex = 0;
                    msgID = BitConverter.ToInt32(args.Buffer, nowIndex);
                    nowIndex += 4;
                    msgLength = BitConverter.ToInt32(args.Buffer, nowIndex);
                    nowIndex += 4;
                    BaseMsg msg = null;
                    switch (msgID)
                    {
                        case 1001:
                            msg = new PlayerMsg();
                            msg.Reading(args.Buffer, nowIndex);
                            break;
                    }
                    if (msg != null)
                        receiveQueue.Enqueue(msg);
                }
                if (clientSocket != null && !isClose)
                {
                    args.SetBuffer(0, cacheBytes.Length);
                    clientSocket.ReceiveFromAsync(args);
                }
            }
            catch (SocketException s)
            {
                print("接收消息出错" + s.SocketErrorCode + s.Message);
                Close();
            }
            catch (Exception e)
            {
                print("接收消息出错(可能是反序列化问题)" + e.Message);
                Close();
            }
        }
        else
        {
            print("接收消息失败" + args.SocketError);
        }
    }
    public void Send(BaseMsg msg)
    {
        try
        {
            if(clientSocket != null && !isClose)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                byte[] bytes = msg.Writing();
                args.SetBuffer(bytes, 0, bytes.Length);
                args.Completed += SendToCallBack;
                args.RemoteEndPoint = serverIpPoint;
                clientSocket.SendToAsync(args);
            }
        }
        catch (SocketException s)
        {
            print("发送消息出错" + s.SocketErrorCode + s.Message);
        }
        catch (Exception e)
        {
            print("发送消息出错(可能是序列化问题)" + e.Message);
        }
    }
    private void SendToCallBack(object o, SocketAsyncEventArgs args)
    {
        if (args.SocketError != SocketError.Success)
            print("发送消息失败" + args.SocketError);
    }
    public void Close()
    {
        if (clientSocket != null)
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
