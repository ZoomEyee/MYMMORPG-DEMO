using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpAsyncServer
{
    class ClientSocket
    {
        public Socket socket;
        public int clientID;
        private static int CLIENT_BEGIN_ID = 1;
        private byte[] cacheBytes = new byte[1024];
        private int cacheNum = 0;
        private long frontTime = -1;
        private static int TIME_OUT_TIME = 10;
        public ClientSocket(Socket socket)
        {
            this.clientID = CLIENT_BEGIN_ID++;
            this.socket = socket;
            this.socket.BeginReceive(cacheBytes, cacheNum, cacheBytes.Length, SocketFlags.None, ReceiveCallBack, null);
            ThreadPool.QueueUserWorkItem(CheckTimeOut);
        }
        private void CheckTimeOut(object obj)
        {
            while (this.socket != null && this.socket.Connected)
            {
                if (frontTime != -1 &&
                DateTime.Now.Ticks / TimeSpan.TicksPerSecond - frontTime >= TIME_OUT_TIME)
                {
                    Program.serverSocket.CloseClientSocket(this);
                    break;
                }
                Thread.Sleep(5000);
            }
        }        
        public void Send(BaseMsg msg)
        {
            if(socket != null && socket.Connected)
            {
                byte[] bytes = msg.Writing();
                socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallBack, null);
            }
            else
            {
                Program.serverSocket.CloseClientSocket(this);
            }
        }
        private void SendCallBack(IAsyncResult result)
        {
            try
            {
                if (socket != null && socket.Connected)
                    this.socket.EndSend(result);
                else
                    Program.serverSocket.CloseClientSocket(this);
            }
            catch (SocketException e)
            {
                Console.WriteLine("发送失败" + e.SocketErrorCode + e.Message);
                Program.serverSocket.CloseClientSocket(this);
            }
        }
        private void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                if (this.socket != null && this.socket.Connected)
                {
                    int num = this.socket.EndReceive(result);
                    HandleReceiveMsg(num);
                    this.socket.BeginReceive(cacheBytes, cacheNum, cacheBytes.Length - cacheNum, SocketFlags.None, ReceiveCallBack, this.socket);
                }
                else
                {
                    Console.WriteLine("没有连接，不用再收消息了");
                    Program.serverSocket.CloseClientSocket(this);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("接受消息错误" + e.SocketErrorCode + e.Message);
                Program.serverSocket.CloseClientSocket(this);
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
                        case 1003:
                            baseMsg = new QuitMsg();
                            break;
                        case 999:
                            baseMsg = new HeartMsg();
                            break;
                    }
                    if (baseMsg != null)
                        ThreadPool.QueueUserWorkItem(MsgHandle, baseMsg);
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
        private void MsgHandle(object obj)
        {
            switch (obj)
            {
                case PlayerMsg msg:
                    PlayerMsg playerMsg = msg as PlayerMsg;
                    Console.WriteLine(playerMsg.playerID);
                    Console.WriteLine(playerMsg.playerData.name);
                    Console.WriteLine(playerMsg.playerData.lev);
                    Console.WriteLine(playerMsg.playerData.atk);
                    break;
                case QuitMsg msg:
                    Program.serverSocket.CloseClientSocket(this);
                    break;
                case HeartMsg msg:
                    frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                    Console.WriteLine("收到心跳消息");
                    break;
            }
        }
        public void Close()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
    }
}
