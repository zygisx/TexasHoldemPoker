using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PokerClient
{
    class Client
    {

        
        private TcpClient clientSocket = new TcpClient();
        private NetworkStream serverStream;

        public GameWindow Window
        {
            get;
            set;
        }

        public Client()
        {
        }

        public void ConnectToServer(string ip, int port, string nick)
        {
            if (string.IsNullOrEmpty(nick))
                return;
            this.clientSocket.Connect(ip, port);
            this.SendData(nick);
        }

        public void CloseConnection()
        {
            clientSocket.Close();
        }

        public void SendData(string dataTosend)
        {
            if (string.IsNullOrEmpty(dataTosend))
                return;
            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(dataTosend);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
            WaitForDataReceive();
        }

        public void WaitForDataReceive()
        {
            byte[] buffer = new byte[1024];
            serverStream = clientSocket.GetStream();
            //System.Console.WriteLine("PX");
            serverStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        public void ReadCallback(IAsyncResult result)
        {
            NetworkStream serverStream = clientSocket.GetStream();
            try
            {
                int read = serverStream.EndRead(result);
                if (read == 0)
                {
                    serverStream.Close();
                    //_clientSocket.Close();
                    return;
                }

                byte[] buffer = result.AsyncState as byte[];
                string data = Encoding.Default.GetString(buffer, 0, read);

                ////MainWindow.PutMessage("Received: " + data + "\n");
                //do the job with the data here
                //send the data back to client.
                //Byte[] sendBytes = Encoding.ASCII.GetBytes("Processed " + data);
                //networkStream.Write(sendBytes, 0, sendBytes.Length);
                //networkStream.Flush();

                //Server.parent.PutMessage("Received: " + data);
            }
            catch (Exception ex)
            {
                throw;
            }

            this.WaitForDataReceive();
        }

    }
}
