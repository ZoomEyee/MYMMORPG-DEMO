using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpServer
{
    class ServerSocket
    {
        private Socket socket;
        private bool isClose;
        private Dictionary<string, Client> clientDic = new Dictionary<string, Client>();
        public void Start(string ip, int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(ipPoint);
                isClose = false;
                ThreadPool.QueueUserWorkItem(ReceiveMsg);
                ThreadPool.QueueUserWorkItem(CheckTimeOut);
            }
            catch (Exception e)
            {
                Console.WriteLine("UDP开启出错" + e.Message);
            }
        }

        private void CheckTimeOut(object obj)
        {
            long nowTime = 0;
            List<string> delList = new List<string>();
            while (true)
            {
                Thread.Sleep(30000);
                nowTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                foreach (Client c in clientDic.Values)
                {
                    if (nowTime - c.frontTime >= 10)
                        delList.Add(c.clientStrID);
                }
                for (int i = 0; i < delList.Count; i++)
                    RemoveClient(delList[i]);
                delList.Clear();
            }
        }

        private void ReceiveMsg(object obj)
        {
            byte[] bytes = new byte[512];
            EndPoint ipPoint = new IPEndPoint(IPAddress.Any, 0);
            string strID = "";
            string ip;
            int port;
            while (!isClose)
            {
                if(socket.Available > 0)
                {
                    lock(socket)
                        socket.ReceiveFrom(bytes, ref ipPoint);
                    ip = (ipPoint as IPEndPoint).Address.ToString();
                    port = (ipPoint as IPEndPoint).Port;
                    strID = ip + port;
                    if (clientDic.ContainsKey(strID))
                        clientDic[strID].ReceiveMsg(bytes);
                    else//如果没有 直接添加并且处理消息
                    {
                        clientDic.Add(strID, new Client(ip, port));
                        clientDic[strID].ReceiveMsg(bytes);
                    }
                }
            }
        }
        public void SendTo(BaseMsg msg, IPEndPoint ipPoint)
        {
            try
            {
                lock (socket)
                    socket.SendTo(msg.Writing(), ipPoint);
            }
            catch (SocketException s)
            {
                Console.WriteLine("发消息出现问题" + s.SocketErrorCode + s.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("发送消息出问题（可能是序列化问题）" + e.Message);
            }
            
        }

        public void Broadcast(BaseMsg msg)
        {
            foreach (Client c in clientDic.Values)
            {
                SendTo(msg, c.clientIPandPort);
            }
        }

        public void Close()
        {
            if(socket != null)
            {
                isClose = true;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }

        public void RemoveClient(string clientID)
        {
            if(clientDic.ContainsKey(clientID))
            {
                Console.WriteLine("客户端{0}被移除了" + clientDic[clientID].clientIPandPort);
                clientDic.Remove(clientID);
            }
        }
    }
}
