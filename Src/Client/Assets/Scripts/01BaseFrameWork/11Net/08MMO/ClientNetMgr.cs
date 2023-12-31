using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Network
{
    class ClientNetMgr : MonoSingleton<ClientNetMgr>
    {
        #region 默认参数
        const int DEF_POLL_INTERVAL_MILLISECONDS = 100; //网络线程间隔毫秒
        const int DEF_TRY_CONNECT_TIMES = 3;            //重连次数
        const int DEF_RECEIVE_BUFFER_SIZE = 64 * 1024;  //receiveStream中字节的初始大小
        const int DEF_PACKAGE_HEADER_LENGTH = 4;        //包长度
        const int DEF_SEND_PING_INTERVAL = 30;          //包延迟
        const int NetConnectTimeout = 10000;            //连接等待毫秒
        const int DEF_LOAD_WHEEL_MILLISECONDS = 1000;   //等待几毫秒显示加载轮
        const int NetReconnectPeriod = 10;              //重新连接时间
        #endregion
        #region 错误代码
        public const int NET_ERROR_UNKNOW_PROTOCOL = 2;         //协议错误
        public const int NET_ERROR_SEND_EXCEPTION = 1000;       //发送异常
        public const int NET_ERROR_ILLEGAL_PACKAGE = 1001;      //接受到错误数据包
        public const int NET_ERROR_ZERO_BYTE = 1002;            //收发0字节
        public const int NET_ERROR_PACKAGE_TIMEOUT = 1003;      //收包超时
        public const int NET_ERROR_PROXY_TIMEOUT = 1004;        //proxy超时
        public const int NET_ERROR_FAIL_TO_CONNECT = 1005;      //3次连接不上
        public const int NET_ERROR_PROXY_ERROR = 1006;          //proxy重启
        public const int NET_ERROR_ON_DESTROY = 1007;           //结束的时候，关闭网络连接
        public const int NET_ERROR_ON_KICKOUT = 25;             //被踢了
        #endregion
        
        public delegate void ConnectEventHandler(int result, string reason);
        public delegate void ExpectPackageEventHandler();

        public event ConnectEventHandler OnConnect;
        public event ConnectEventHandler OnDisconnect;
        public event ExpectPackageEventHandler OnExpectPackageTimeout;
        public event ExpectPackageEventHandler OnExpectPackageResume;

        private IPEndPoint address;
        private Socket clientSocket;
        private MemoryStream sendStream = new MemoryStream();
        private MemoryStream receiveStream = new MemoryStream(DEF_RECEIVE_BUFFER_SIZE);
        //private Queue<NetMessage> sendQueue = new Queue<NetMessage>();
        //public PackageHandler packageHandler = new PackageHandler(null);

        private int sendOffset = 0;
        private int retryTimes = 0;
        private int retryTimesTotal = DEF_TRY_CONNECT_TIMES;
        private float lastSendTime = 0;
               
        private bool connecting = false;
        public bool Connected
        {
            get
            {
                return (clientSocket != default(Socket)) ? clientSocket.Connected : false;
            }
        }
        public bool running { get; set; }
        public ClientNetMgr()
        {
            running = true;
            //MessageDistributer.Instance.ThrowException = true;
        }

        protected virtual void RaiseConnected(int result, string reason)
        {
            ConnectEventHandler handler = OnConnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }
        public virtual void RaiseDisonnected(int result, string reason = "")
        {
            ConnectEventHandler handler = OnDisconnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }
        protected virtual void RaiseExpectPackageTimeout()
        {
            ExpectPackageEventHandler handler = OnExpectPackageTimeout;
            if (handler != null)
            {
                handler();
            }
        }
        protected virtual void RaiseExpectPackageResume()
        {
            ExpectPackageEventHandler handler = OnExpectPackageResume;
            if (handler != null)
            {
                handler();
            }
        } 
        public void Init(string serverIP, int port)
        {
            address = new IPEndPoint(IPAddress.Parse(serverIP), port);
        }       
        public void Connect(int times = DEF_TRY_CONNECT_TIMES)//这里times貌似没有用到
        {
            if (connecting)
                return;
            if (clientSocket != null)
                clientSocket.Close();
            if (address == default(IPEndPoint))
                throw new Exception("Please Init first.");
            connecting = true;
            lastSendTime = 0;
            DoConnect();
        }
        private void DoConnect()
        {
            try
            {
                if (clientSocket != null)
                    clientSocket.Close();
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Blocking = true;//开启阻塞模式,调用Send或者Receive会卡住线程

                Debug.Log(string.Format("Connect[{0}] to server {1}", retryTimes, address) + "\n");
                IAsyncResult result = clientSocket.BeginConnect(address, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(NetConnectTimeout);//等待指定时间进行连接,会阻塞线程,如果没指定时间会一直阻塞线程(异步却还要等待是要保证能连接上)
                if (success)
                    clientSocket.EndConnect(result);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)//连接请求被服务器拒绝
                    CloseConnection(NET_ERROR_FAIL_TO_CONNECT);
                Debug.LogErrorFormat("DoConnect SocketException:[{0},{1},{2}]{3} ", ex.ErrorCode, ex.SocketErrorCode, ex.NativeErrorCode, ex.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("DoConnect Exception:" + e.ToString() + "\n");
            }

            if (clientSocket.Connected)
            {
                clientSocket.Blocking = false;//连接时设置为阻塞,连接成功后设置为非阻塞,避免后续的线程卡死影响游戏体验
                RaiseConnected(0, "Success");
            }
            else
            {
                retryTimes++;
                if (retryTimes >= retryTimesTotal)
                    RaiseConnected(1, "Cannot connect to server");
            }
            connecting = false;
        }
        public void CloseConnection(int errCode)
        {
            Debug.LogWarning("CloseConnection(), errorCode: " + errCode.ToString());
            connecting = false;
            if (clientSocket != null)
                clientSocket.Close();
            //MessageDistributer.Instance.Clear();
            //sendQueue.Clear();
            receiveStream.Position = 0;
            sendStream.Position = sendOffset = 0;

            switch (errCode)
            {
                case NET_ERROR_UNKNOW_PROTOCOL:                       
                        running = false;
                    break;
                case NET_ERROR_FAIL_TO_CONNECT:
                case NET_ERROR_PROXY_TIMEOUT:
                case NET_ERROR_PROXY_ERROR:
                    //NetworkManager.Instance.dropCurMessage();
                    //NetworkManager.Instance.Connect();
                    break;
                case NET_ERROR_ON_KICKOUT:
                case NET_ERROR_ZERO_BYTE:
                case NET_ERROR_ILLEGAL_PACKAGE:
                case NET_ERROR_SEND_EXCEPTION:
                case NET_ERROR_PACKAGE_TIMEOUT:
                default:
                    lastSendTime = 0;
                    RaiseDisonnected(errCode);
                    break;
            }
        }
        public void SendMessage(/*NetMessage message*/)
        {
            if (!running)
                return;
            if (!Connected)
            {
                receiveStream.Position = 0;
                sendStream.Position = sendOffset = 0;
                Connect();
                Debug.Log("Connect Server before Send Message!");
                return;
            }
            //sendQueue.Enqueue(message);
            if (lastSendTime == 0)
                lastSendTime = Time.time;
        }
        private bool KeepConnect()
        {
            if (connecting)
                return false;
            if (address == null)
                return false;
            if (Connected)
                return true;
            if (retryTimes < retryTimesTotal)
                Connect();
            return false;
        }
        private bool ProcessReceive()
        {
            bool ret = false;
            try
            {
                if (clientSocket.Blocking)
                    Debug.Log("this.clientSocket.Blocking = true\n");
                bool error = clientSocket.Poll(0, SelectMode.SelectError);//Poll方法用于在非阻塞模式下检查套接字的状态(等待时间,SelectMode)
                if (error)
                {
                    Debug.Log("ProcessRecv Poll SelectError\n");
                    CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }
                ret = clientSocket.Poll(0, SelectMode.SelectRead);//为什么要先判断Socket是否可读,而不直接接收呢,因为每帧都检测接收会更消耗性能,使用Poll方法可以更加高效
                if (ret)
                {
                    int n = clientSocket.Receive(receiveStream.GetBuffer(), 0, receiveStream.Capacity, SocketFlags.None);
                    if (n <= 0)
                    {
                        CloseConnection(NET_ERROR_ZERO_BYTE);
                        return false;
                    }
                    //packageHandler.ReceiveData(receiveStream.GetBuffer(), 0, n);//将接受到字节数组反序列化成消息然后使用MessageDistributer分发到指定模块
                }
            }
            catch (Exception e)
            {
                Debug.Log("ProcessReceive exception:" + e.ToString() + "\n");
                this.CloseConnection(NET_ERROR_ILLEGAL_PACKAGE);
                return false;
            }
            return true;
        }
        private bool ProcessSend()
        {
            bool ret = false;
            try
            {
                if (clientSocket.Blocking)
                    Debug.Log("this.clientSocket.Blocking = true\n");
                bool error = clientSocket.Poll(0, SelectMode.SelectError);
                if (error)
                {
                    Debug.Log("ProcessSend Poll SelectError\n");
                    CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }
                ret = clientSocket.Poll(0, SelectMode.SelectWrite);
                if (ret)
                {
                    if (sendStream.Position > sendOffset)
                    {
                        int bufsize = (int)(sendStream.Position - sendOffset);
                        int n = clientSocket.Send(sendStream.GetBuffer(), sendOffset, bufsize, SocketFlags.None);
                        if (n <= 0)
                        {
                            CloseConnection(NET_ERROR_ZERO_BYTE);
                            return false;
                        }
                        sendOffset += n;
                        if (sendOffset >= sendStream.Position)
                        {
                            sendOffset = 0;
                            sendStream.Position = 0;
                            //sendQueue.Dequeue();//remove message when send complete
                        }
                    }
                    else
                    {
                        /*if (this.sendQueue.Count > 0)
                        {
                            NetMessage message = this.sendQueue.Peek();
                            byte[] package = PackageHandler.PackMessage(message);
                            sendStream.Write(package, 0, package.Length);
                        }*/
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("ProcessSend exception:" + e.ToString() + "\n");
                CloseConnection(NET_ERROR_SEND_EXCEPTION);
                return false;
            }
            return true;
        }
        private void ProceeMessage()
        {
            //MessageDistributer.Instance.Distribute();
        }

        public void Reset()
        {
            //MessageDistributer.Instance.Clear();
            //this.sendQueue.Clear();
            this.sendOffset = 0;
            this.connecting = false;
            this.retryTimes = 0;
            this.lastSendTime = 0;
            this.OnConnect = null;
            this.OnDisconnect = null;
            this.OnExpectPackageTimeout = null;
            this.OnExpectPackageResume = null;
        }
        public void Update()
        {
            if (!running)
                return;
            if (KeepConnect())//保证断线重连
            {
                if (ProcessReceive())//判断套接字状态,每一帧先看看有没有数据,套接字状态正常则先接收数据(只接受消息,不处理消息,处理消息是ProceeMessage)
                {
                    if (Connected)//接受完可能会断线,所以再检测一次是否有连接
                    {
                        ProcessSend();//发送消息
                        ProceeMessage();//处理信息,一次性分发队列中的所有消息
                    }
                }
            }
        }
        public void OnDestroy()
        {
            Debug.Log("OnDestroy NetworkManager.");
            CloseConnection(NET_ERROR_ON_DESTROY);
        }
    }
}


