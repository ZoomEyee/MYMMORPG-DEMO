using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPSever
{
    class ServerSocket
    {
        //服务端Socket
        public Socket socket;
        //客户端连接的所有Socket
        public Dictionary<int, ClientSocket> clientDic = new Dictionary<int, ClientSocket>();

        //有待移除的客户端socket 避免 在foreach时直接从字典中移除 出现问题
        private List<ClientSocket> delList = new List<ClientSocket>();

        private bool isClose;

        //开启服务器端
        public void Start(string ip, int port, int num)
        {
            isClose = false;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(ipPoint);
            socket.Listen(num);
            ThreadPool.QueueUserWorkItem(Accept);
            ThreadPool.QueueUserWorkItem(Receive);
        }

        //关闭服务器端
        public void Close()
        {
            isClose = true;
            foreach (ClientSocket client in clientDic.Values)
            {
                client.Close();
            }
            clientDic.Clear();

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }

        //接受客户端连入
        private void Accept(object obj)
        {
            while (!isClose)
            {
                try
                {
                    //连入一个客户端
                    Socket clientSocket = socket.Accept();
                    ClientSocket client = new ClientSocket(clientSocket);
                    lock(clientDic)
                        clientDic.Add(client.clientID, client);
                }
                catch (Exception e)
                {
                    Console.WriteLine("客户端连入报错" + e.Message);
                }
            }
        }
        //接收客户端消息
        private void Receive(object obj)
        {
            while (!isClose)
            {
                if(clientDic.Count > 0)
                {
                    lock (clientDic)
                    {
                        foreach (ClientSocket client in clientDic.Values)
                        {
                            client.Receive();
                        }

                        CloseDelListSocket();
                    }
                    
                }
            }
        }

        public void Broadcast(BaseMsg info)
        {
            lock (clientDic)
            {
                foreach (ClientSocket client in clientDic.Values)
                {
                    client.Send(info);
                }
            }
                
        }

        //添加待移除的 socket内容
        public void AddDelSocket(ClientSocket socket)
        {
            if (!delList.Contains(socket))
                delList.Add(socket);
        }

        ////判断有没有 断开连接的 把其 移除
        public void CloseDelListSocket()
        {
            //判断有没有 断开连接的 把其 移除
            for (int i = 0; i < delList.Count; i++)
                CloseClientSocket(delList[i]);
            delList.Clear();
        }

        //关闭客户端连接的 从字典中移除
        public void CloseClientSocket(ClientSocket socket)
        {
            lock (clientDic)
            {
                socket.Close();
                if (clientDic.ContainsKey(socket.clientID))
                {
                    clientDic.Remove(socket.clientID);
                    Console.WriteLine("客户端{0}主动断开连接了", socket.clientID);
                }
            }
        }
    }
}
