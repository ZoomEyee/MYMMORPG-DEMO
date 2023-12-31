using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpServer
{
    class ClientSocket
    {
        public Socket socket;
        public bool Connected => socket.Connected;
        public int clientID;
        private static int CLIENT_BEGIN_ID = 1;
        private byte[] cacheBytes = new byte[1024 * 1024];
        private int cacheNum = 0;
        private long frontTime = -1;
        private static int TIME_OUT_TIME = 10;
        public ClientSocket(Socket socket)
        {
            this.clientID = CLIENT_BEGIN_ID;
            this.socket = socket;
            ++CLIENT_BEGIN_ID;
        }
        private void CheckTimeOut()
        {
            if (frontTime != -1 &&
            DateTime.Now.Ticks / TimeSpan.TicksPerSecond - frontTime >= TIME_OUT_TIME)
            {
                Program.socket.AddDelSocket(this);
            }
        }        
        public void Send(BaseMsg info)
        {
            if (Connected)
            {
                try
                {
                    socket.Send(info.Writing());
                }
                catch (Exception e)
                {
                    Console.WriteLine("发消息出错" + e.Message);
                    Program.socket.AddDelSocket(this);
                }
            }
            else
                Program.socket.AddDelSocket(this);
        }
        public void Receive()
        {
            if (!Connected)
            {
                Program.socket.AddDelSocket(this);
                return;
            }
            try
            {
                if (socket.Available > 0)
                {
                    byte[] result = new byte[1024 * 5];
                    int receiveNum = socket.Receive(result);
                    HandleReceiveMsg(result, receiveNum);
                } 
                CheckTimeOut();
            }
            catch (Exception e)
            {
                Console.WriteLine("收消息出错" + e.Message);
                Program.socket.AddDelSocket(this);
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
            BaseMsg msg = obj as BaseMsg;
            if (msg is PlayerMsg)
            {
                PlayerMsg playerMsg = msg as PlayerMsg;
                Console.WriteLine(playerMsg.playerID);
                Console.WriteLine(playerMsg.playerData.name);
                Console.WriteLine(playerMsg.playerData.lev);
                Console.WriteLine(playerMsg.playerData.atk);
            }
            else if (msg is QuitMsg)
            {
                Program.socket.AddDelSocket(this);
            }
            else if (msg is HeartMsg)
            {
                frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                Console.WriteLine("收到心跳消息");
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
