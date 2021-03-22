using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ChessServer
{
    class Server
    {
        private Dictionary<Player, Socket> clients = new Dictionary<Player, Socket>();

        private Socket socket;
        private IPEndPoint localEndPoint;

        public Server(int port) 
        {
            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            this.localEndPoint = new IPEndPoint(IPAddress.Any, port);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            this.socket.Bind(this.localEndPoint);
            this.socket.Listen();
            Console.WriteLine("Server ready and waiting for connections...");
            Socket white = this.socket.Accept();
            this.clients.Add(Player.WHITE, white);
            Console.WriteLine("White player has connected!");
            Socket black = this.socket.Accept();
            this.clients.Add(Player.BLACK, black);
            Console.WriteLine("Black player has connected!");
        }

        public string WaitForMessage(Player player, int timeout, out MessageType messageType)
        {
            byte[] bytes = new byte[1024];
            string data = null;
            Socket s = this.clients[player];
            while(true)
            {
                int bytesRec = s.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (bytesRec == 5 + BitConverter.ToInt32(bytes, 1))  
                {
                    return this.DecodeMessage(bytes, out messageType);
                }
            }
        }

        public void SendMessage(Player recipient, MessageType messageType, string message)
        {
            byte[] bytes = this.EncodeMessage(messageType, message);
            this.clients[recipient].Send(bytes);
        }

        public void BroadcastMessage(MessageType messageType, string message)
        {
            byte[] bytes = this.EncodeMessage(messageType, message);
            foreach (Socket socket in this.clients.Values)
            {
                socket.Send(bytes);
            }
        }

        private byte[] EncodeMessage(MessageType messageType, string message)
        {
            int contentLength = message.Length;
            // BitConverter.GetBytes(contentLength);
            byte[] msg = new byte[5 + contentLength];

            msg[0] = (byte)messageType;
            BitConverter.GetBytes(contentLength).CopyTo(msg, 1);
            Encoding.ASCII.GetBytes(message).CopyTo(msg, 5);

            return msg;
        }

        private string DecodeMessage(byte[] bytes, out MessageType messageType)
        {
            messageType = (MessageType)bytes[0];
            int contentLength = BitConverter.ToInt32(bytes, 1);
            return Encoding.ASCII.GetString(bytes, 5, contentLength);
        }
    }
}
