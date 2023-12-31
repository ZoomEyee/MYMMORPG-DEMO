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
        #region Ĭ�ϲ���
        const int DEF_POLL_INTERVAL_MILLISECONDS = 100; //�����̼߳������
        const int DEF_TRY_CONNECT_TIMES = 3;            //��������
        const int DEF_RECEIVE_BUFFER_SIZE = 64 * 1024;  //receiveStream���ֽڵĳ�ʼ��С
        const int DEF_PACKAGE_HEADER_LENGTH = 4;        //������
        const int DEF_SEND_PING_INTERVAL = 30;          //���ӳ�
        const int NetConnectTimeout = 10000;            //���ӵȴ�����
        const int DEF_LOAD_WHEEL_MILLISECONDS = 1000;   //�ȴ���������ʾ������
        const int NetReconnectPeriod = 10;              //��������ʱ��
        #endregion
        #region �������
        public const int NET_ERROR_UNKNOW_PROTOCOL = 2;         //Э�����
        public const int NET_ERROR_SEND_EXCEPTION = 1000;       //�����쳣
        public const int NET_ERROR_ILLEGAL_PACKAGE = 1001;      //���ܵ��������ݰ�
        public const int NET_ERROR_ZERO_BYTE = 1002;            //�շ�0�ֽ�
        public const int NET_ERROR_PACKAGE_TIMEOUT = 1003;      //�հ���ʱ
        public const int NET_ERROR_PROXY_TIMEOUT = 1004;        //proxy��ʱ
        public const int NET_ERROR_FAIL_TO_CONNECT = 1005;      //3�����Ӳ���
        public const int NET_ERROR_PROXY_ERROR = 1006;          //proxy����
        public const int NET_ERROR_ON_DESTROY = 1007;           //������ʱ�򣬹ر���������
        public const int NET_ERROR_ON_KICKOUT = 25;             //������
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
        public void Connect(int times = DEF_TRY_CONNECT_TIMES)//����timesò��û���õ�
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
                clientSocket.Blocking = true;//��������ģʽ,����Send����Receive�Ῠס�߳�

                Debug.Log(string.Format("Connect[{0}] to server {1}", retryTimes, address) + "\n");
                IAsyncResult result = clientSocket.BeginConnect(address, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(NetConnectTimeout);//�ȴ�ָ��ʱ���������,�������߳�,���ûָ��ʱ���һֱ�����߳�(�첽ȴ��Ҫ�ȴ���Ҫ��֤��������)
                if (success)
                    clientSocket.EndConnect(result);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)//�������󱻷������ܾ�
                    CloseConnection(NET_ERROR_FAIL_TO_CONNECT);
                Debug.LogErrorFormat("DoConnect SocketException:[{0},{1},{2}]{3} ", ex.ErrorCode, ex.SocketErrorCode, ex.NativeErrorCode, ex.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("DoConnect Exception:" + e.ToString() + "\n");
            }

            if (clientSocket.Connected)
            {
                clientSocket.Blocking = false;//����ʱ����Ϊ����,���ӳɹ�������Ϊ������,����������߳̿���Ӱ����Ϸ����
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
                bool error = clientSocket.Poll(0, SelectMode.SelectError);//Poll���������ڷ�����ģʽ�¼���׽��ֵ�״̬(�ȴ�ʱ��,SelectMode)
                if (error)
                {
                    Debug.Log("ProcessRecv Poll SelectError\n");
                    CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }
                ret = clientSocket.Poll(0, SelectMode.SelectRead);//ΪʲôҪ���ж�Socket�Ƿ�ɶ�,����ֱ�ӽ�����,��Ϊÿ֡�������ջ����������,ʹ��Poll�������Ը��Ӹ�Ч
                if (ret)
                {
                    int n = clientSocket.Receive(receiveStream.GetBuffer(), 0, receiveStream.Capacity, SocketFlags.None);
                    if (n <= 0)
                    {
                        CloseConnection(NET_ERROR_ZERO_BYTE);
                        return false;
                    }
                    //packageHandler.ReceiveData(receiveStream.GetBuffer(), 0, n);//�����ܵ��ֽ����鷴���л�����ϢȻ��ʹ��MessageDistributer�ַ���ָ��ģ��
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
            if (KeepConnect())//��֤��������
            {
                if (ProcessReceive())//�ж��׽���״̬,ÿһ֡�ȿ�����û������,�׽���״̬�������Ƚ�������(ֻ������Ϣ,��������Ϣ,������Ϣ��ProceeMessage)
                {
                    if (Connected)//��������ܻ����,�����ټ��һ���Ƿ�������
                    {
                        ProcessSend();//������Ϣ
                        ProceeMessage();//������Ϣ,һ���Էַ������е�������Ϣ
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


