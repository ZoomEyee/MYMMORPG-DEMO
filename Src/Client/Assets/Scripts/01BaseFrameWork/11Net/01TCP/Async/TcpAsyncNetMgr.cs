using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TcpAsyncNetMgr : MonoSingleton<TcpAsyncNetMgr>
{
    public int SEND_HEART_MSG_TIME = 2;
    private Socket clientSocket;
    private Queue<BaseMsg> receiveMsgQueue = new Queue<BaseMsg>();
    private byte[] cacheBytes = new byte[1024 * 1024];
    private int cacheNum = 0;    
    private HeartMsg hearMsg = new HeartMsg();
    public void Send(BaseMsg msg)
    {
        if (this.clientSocket != null && this.clientSocket.Connected)
        {
            byte[] bytes = msg.Writing();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);
            args.Completed += (socket, args) =>
            {
                if (args.SocketError != SocketError.Success)
                {
                    print("发送消息失败" + args.SocketError);
                    Close();
                }

            };
            this.clientSocket.SendAsync(args);
        }
        else
        {
            Close();
        }
    }
    public void SendBytes(byte[] bytes)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.SetBuffer(bytes, 0, bytes.Length);
        args.Completed += (socket, args) =>
        {
            if (args.SocketError != SocketError.Success)
            {
                print("发送消息失败" + args.SocketError);
                Close();
            }
        };
        this.clientSocket.SendAsync(args);
    }
    private void SendHeartMsg()
    {
        if (clientSocket != null && clientSocket.Connected)
            Send(hearMsg);
    }
    public void Connect(string ip, int port)
    {
        if (clientSocket != null && clientSocket.Connected)
            return;

        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = ipPoint;
        args.Completed += (socket, args) =>
        {
            if(args.SocketError == SocketError.Success)
            {
                print("连接成功");
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(cacheBytes, 0, cacheBytes.Length);
                receiveArgs.Completed += ReceiveCallBack;
                this.clientSocket.ReceiveAsync(receiveArgs);
            }
            else
            {
                print("连接失败" + args.SocketError);
            }
        };
        clientSocket.ConnectAsync(args);
    }
    private void ReceiveCallBack(object obj, SocketAsyncEventArgs args)
    {
        if(args.SocketError == SocketError.Success)
        {
            HandleReceiveMsg(args.BytesTransferred);
            args.SetBuffer(cacheNum, args.Buffer.Length - cacheNum);
            if (this.clientSocket != null && this.clientSocket.Connected)
                clientSocket.ReceiveAsync(args);
            else
                Close();
        }
        else
        {
            print("接受消息出错" + args.SocketError);
            Close();
        }
    }
    private void HandleReceiveMsg(int receiveNum)
    {
        int msgID = 0;
        int msgLength = 0;
        int nowIndex = 0;
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
                        baseMsg = new PlayerMsg();
                        baseMsg.Reading(cacheBytes, nowIndex);
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
        if(clientSocket != null)
        {
            QuitMsg msg = new QuitMsg();
            clientSocket.Send(msg.Writing());
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Disconnect(false);
            clientSocket.Close();
            clientSocket = null;
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
            BaseMsg baseMsg = receiveMsgQueue.Dequeue();
            switch (baseMsg)
            {
                case PlayerMsg msg:
                    print(msg.playerID);
                    print(msg.playerData.name);
                    print(msg.playerData.lev);
                    print(msg.playerData.atk);
                    break;
            }
        }
    }
    private void OnDestroy()
    {
        Close();
    }
}
