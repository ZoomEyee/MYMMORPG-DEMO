using System;

namespace TcpServer
{
    class Program
    {
        public static ServerSocket socket;
        static void Main(string[] args)
        {
            socket = new ServerSocket();
            socket.Start("127.0.0.1", 8080, 1024);
            Console.WriteLine("服务器开启成功");
            while (true)
            {
                string input = Console.ReadLine();
                if(input == "Quit")
                {
                    socket.Close();
                }
                else if( input.Substring(0,2) == "B:" )
                {
                    if(input.Substring(2) == "1001")
                    {
                        PlayerMsg msg = new PlayerMsg();
                        msg.playerID = 9876;
                        msg.playerData = new PlayerData();
                        msg.playerData.name = "服务器端发来的消息";
                        msg.playerData.lev = 99;
                        msg.playerData.atk = 80;
                        socket.Broadcast(msg);
                    }
                }
            }
        }
    }
}
