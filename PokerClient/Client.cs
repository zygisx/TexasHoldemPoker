using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using CommonClassLibrary;



namespace PokerClient
{
    public class Client
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

            // Wait for game data
        }

        public void CloseConnection()
        {
            clientSocket.Close();
        }

        public void WaitForDataReceive()
        {
            byte[] buffer = new byte[1024];
            serverStream = clientSocket.GetStream();
            serverStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        public void ReadCallback(IAsyncResult result)
        {
            NetworkStream serverStream = clientSocket.GetStream();
            MemoryStream memStream = new MemoryStream();
            try
            {
                int read = serverStream.EndRead(result);
                if (read == 0)
                {
                    //serverStream.Close();
                    return;
                }
                
                // get result from network streaM
                byte[] buffer = result.AsyncState as byte[];
                memStream.Write(buffer, 0, read);
                memStream.Position = 0;

                // Deserialization
                BinaryFormatter bf = new BinaryFormatter();
                ClientsGame game = (ClientsGame)bf.Deserialize(memStream);
                
                // Game stuff
                System.Console.WriteLine("Stuff");
                Window.Game = game;
                Window.UpdateGame();
                Window.PutLogMessage(game.ActionMade.ToString());
                

                // and again wait for game data
                this.WaitForDataReceive();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                Window.PutLogMessage("ERROR: " + ex);
            }

            
        }

        public void SendGame()
        {
            NetworkStream serverStream = clientSocket.GetStream();
            MemoryStream memStream = new MemoryStream();
            try
            {

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(memStream, Window.Game);

                byte[] buffer = memStream.GetBuffer();
                serverStream.Write(buffer, 0, buffer.Length);
                serverStream.Flush();

               
            }
            catch (Exception ex)
            {
                Window.PutLogMessage(ex.ToString());
            }
        }

        /* deprecated, use only for send players nickname */
        public void SendData(string dataTosend)
        {
            if (string.IsNullOrEmpty(dataTosend))
                return;
            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(dataTosend);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
            //WaitForDataReceive();
        }

    }
}
