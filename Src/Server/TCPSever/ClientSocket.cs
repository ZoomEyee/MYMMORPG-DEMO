using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPSever
{
    class ClientSocket
    {
        private static int CLIENT_BEGIN_ID = 1;
        public int clientID;
        public Socket socket;

        //用于处理分包时 缓存的 字节数组 和 字节数组长度
        private byte[] cacheBytes = new byte[1024 * 1024];
        private int cacheNum = 0;

        //上一次收到消息的时间
        private long frontTime = -1;
        //超时时间
        private static int TIME_OUT_TIME = 10;

        public ClientSocket(Socket socket)
        {
            this.clientID = CLIENT_BEGIN_ID;
            this.socket = socket;
            ++CLIENT_BEGIN_ID;
            //我们现在为了方便大家理解 所以开了一个线程专门计时 但是这种方式比较消耗性能 不建议这样使用
            //ThreadPool.QueueUserWorkItem(CheckTimeOut);
        }

        /// <summary>
        /// 间隔一段时间 检测一次超时 如果超时 就会主动断开该客户端的连接
        /// </summary>
        /// <param name="obj"></param>
        private void CheckTimeOut(/*object obj*/)
        {
            //while (Connected)
            //{
                if (frontTime != -1 &&
                DateTime.Now.Ticks / TimeSpan.TicksPerSecond - frontTime >= TIME_OUT_TIME)
                {
                    Program.socket.AddDelSocket(this);
                    //break;
                }
                //Thread.Sleep(5000);
            //}
        }

        /// <summary>
        /// 是否是连接状态
        /// </summary>
        public bool Connected => socket.Connected;


        //我们应该封装一些方法
        //关闭
        public void Close()
        {
            if(socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
        //发送
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
        //接收
        public void Receive()
        {
            if (!Connected)
            {
                Program.socket.AddDelSocket(this);
                return;
            }
            try
            {
                if(socket.Available > 0)
                {
                    byte[] result = new byte[1024 * 5];
                    int receiveNum = socket.Receive(result);
                    HandleReceiveMsg(result, receiveNum);
                    ////收到数据后 先读取4个字节 转为ID 才知道用哪一个类型去处理反序列化
                    //int msgID = BitConverter.ToInt32(result, 0);
                    //BaseMsg msg = null;
                    //switch (msgID)
                    //{
                    //    case 1001:
                    //        msg = new PlayerMsg();
                    //        msg.Reading(result, 4);
                    //        break;
                    //}
                    //if (msg == null)
                    //    return;
                    //ThreadPool.QueueUserWorkItem(MsgHandle, msg);
                }

                //检测 是否超时 
                CheckTimeOut();
            }
            catch (Exception e)
            {
                Console.WriteLine("收消息出错" + e.Message);
                //解析消息出错 也认为 要把socket断开了
                Program.socket.AddDelSocket(this);
            }
        }

        //处理接受消息 分包、黏包问题的方法
        private void HandleReceiveMsg(byte[] receiveBytes, int receiveNum)
        {
            int msgID = 0;
            int msgLength = 0;
            int nowIndex = 0;

            //收到消息时 应该看看 之前有没有缓存的 如果有的话 我们直接拼接到后面
            receiveBytes.CopyTo(cacheBytes, cacheNum);
            cacheNum += receiveNum;

            while (true)
            {
                //每次将长度设置为-1 是避免上一次解析的数据 影响这一次的判断
                msgLength = -1;
                //处理解析一条消息
                if (cacheNum - nowIndex >= 8)
                {
                    //解析ID
                    msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                    //解析长度
                    msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                }

                if (cacheNum - nowIndex >= msgLength && msgLength != -1)
                {
                    //解析消息体
                    BaseMsg baseMsg = null;
                    switch (msgID)
                    {
                        case 1001:
                            baseMsg = new PlayerMsg();
                            baseMsg.Reading(cacheBytes, nowIndex);
                            break;
                        case 1003:
                            baseMsg = new QuitMsg();
                            //由于该消息没有消息体 所以都不用反序列化
                            break;
                        case 999:
                            baseMsg = new HeartMsg();
                            //由于该消息没有消息体 所以都不用反序列化
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
                    //如果不满足 证明有分包 
                    //那么我们需要把当前收到的内容 记录下来
                    //有待下次接受到消息后 再做处理
                    //receiveBytes.CopyTo(cacheBytes, 0);
                    //cacheNum = receiveNum;
                    //如果进行了 id和长度的解析 但是 没有成功解析消息体 那么我们需要减去nowIndex移动的位置
                    if (msgLength != -1)
                        nowIndex -= 8;
                    //就是把剩余没有解析的字节数组内容 移到前面来 用于缓存下次继续解析
                    Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                    cacheNum = cacheNum - nowIndex;
                    break;
                }
            }

        }

        private void MsgHandle(object obj)
        {
            BaseMsg msg = obj as BaseMsg;
            if(msg is PlayerMsg)
            {
                PlayerMsg playerMsg = msg as PlayerMsg;
                Console.WriteLine(playerMsg.playerID);
                Console.WriteLine(playerMsg.playerData.name);
                Console.WriteLine(playerMsg.playerData.lev);
                Console.WriteLine(playerMsg.playerData.atk);
            }
            else if(msg is QuitMsg)
            {
                //收到断开连接消息 把自己添加到待移除的列表当中
                Program.socket.AddDelSocket(this);
            }
            else if(msg is HeartMsg)
            {
                //收到心跳消息 记录收到消息的时间
                frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                Console.WriteLine("收到心跳消息");
            }
        }

    }
}
