using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpServer
{
    class ServerSocket
    {
        public Socket socket;
        public Dictionary<int, ClientSocket> clientDic = new Dictionary<int, ClientSocket>();
        private List<ClientSocket> delList = new List<ClientSocket>();
        private bool isClose;
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
        private void Accept(object obj)
        {
            while (!isClose)
            {
                try
                {
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
        public void AddDelSocket(ClientSocket socket)
        {
            if (!delList.Contains(socket))
                delList.Add(socket);
        }
        public void CloseDelListSocket()
        {
            for (int i = 0; i < delList.Count; i++)
                CloseClientSocket(delList[i]);
            delList.Clear();
        }
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
