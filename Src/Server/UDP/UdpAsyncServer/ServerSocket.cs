using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpAsyncServer
{
    class ServerSocket
    {
        private Socket socket;
        private bool isClose;
        private Dictionary<string, Client> clientDic = new Dictionary<string, Client>();
        private byte[] cacheBytes = new byte[512];

        public void Start(string ip, int port)
        {
            EndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(ipPoint);
                isClose = false;
                socket.BeginReceiveFrom(cacheBytes, 0, cacheBytes.Length, SocketFlags.None, ref ipPoint, ReceiveMsg, ipPoint);
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

        private void ReceiveMsg(IAsyncResult result)
        {            
            EndPoint ipPoint = result.AsyncState as IPEndPoint;
            
            string ip = (ipPoint as IPEndPoint).Address.ToString();
            int port = (ipPoint as IPEndPoint).Port;
            string strID = ip + port;
            try
            {
                socket.EndReceiveFrom(result, ref ipPoint);
                if (clientDic.ContainsKey(strID))
                    clientDic[strID].ReceiveMsg(cacheBytes);
                else
                {
                    clientDic.Add(strID, new Client(ip, port));
                    clientDic[strID].ReceiveMsg(cacheBytes);
                }
                socket.BeginReceiveFrom(cacheBytes, 0, cacheBytes.Length, SocketFlags.None, ref ipPoint, ReceiveMsg, ipPoint);
            }
            catch (SocketException s)
            {
                Console.WriteLine("接受消息出错" + s.SocketErrorCode + s.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("接受消息出错(非Socket错误)" + e.Message);
            }
        }
        public void SendTo(BaseMsg msg, IPEndPoint ipPoint)
        {
            try
            {
                byte[] bytes = msg.Writing();
                socket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, ipPoint, (result) =>
                {
                    try
                    {
                        socket.EndSendTo(result);
                    }
                    catch (SocketException s)
                    {
                        Console.WriteLine("发消息出现问题" + s.SocketErrorCode + s.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("发送消息出问题（可能是序列化问题）" + e.Message);
                    }
                }, null);
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
            if (socket != null)
            {
                isClose = true;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
        public void RemoveClient(string clientID)
        {
            if (clientDic.ContainsKey(clientID))
            {
                Console.WriteLine("客户端{0}被移除了" + clientDic[clientID].clientIPandPort);
                clientDic.Remove(clientID);
            }
        }
    }
}
