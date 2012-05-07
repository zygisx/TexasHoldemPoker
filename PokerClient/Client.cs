using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

using CommonClassLibrary;



namespace PokerClient
{
    public class Client
    {
        public const string REFUSE_STRING = "false";
        
        private TcpClient clientSocket = new TcpClient();
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private NetworkStream serverStream;

        public State ConnectionState
        {
            get;
            private set;
        }

        public string Name 
        {
            get;
            private set;
        }


        public GameWindow Window
        {
            get;
            set;
        }

        public Client()
        {
        }

        public bool ConnectToServer(string ip, int port, string nick)
        {
            if (string.IsNullOrEmpty(nick))
                return false;
            this.clientSocket.Connect(ip, port);
            allDone.Reset();
            this.SendData(nick);
            this.WaitForNickConfirm();
            if (this.ConnectionState == State.OK)
            {
                this.Name = nick;
                return true;
            }
            else return false;

            // Wait for game data
        }

        public void CloseConnection()
        {
            clientSocket.GetStream().Close();
            clientSocket.Close();
        }

        public void WaitForNickConfirm()
        {
            
            byte[] buffer = new byte[256];
            serverStream = clientSocket.GetStream();
            serverStream.BeginRead(buffer, 0, buffer.Length, NickConfirmCallBack, buffer);
            this.allDone.WaitOne();
        }

        public void NickConfirmCallBack(IAsyncResult result)
        {
            NetworkStream serverStream = clientSocket.GetStream();
            MemoryStream memStream = new MemoryStream();
            try
            {
                int read = serverStream.EndRead(result);
                if (read == 0)
                {
                    serverStream.Close();
                    return;
                }
                // get result from network stream
                byte[] buffer = result.AsyncState as byte[];
                memStream.Write(buffer, 0, read);
                memStream.Position = 0;

                // Deserialization
                BinaryFormatter bf = new BinaryFormatter();
                this.ConnectionState = (State)bf.Deserialize(memStream);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                Window.PutLogMessage("ERROR: " + ex);
            }
            finally
            {
                allDone.Set();
            }
        }

        public void SendGame()
        {
            try
            {
                NetworkStream serverStream = clientSocket.GetStream();
                MemoryStream memStream = new MemoryStream();
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

        public void WaitForDataReceive()
        {
            byte[] buffer = new byte[2048];
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
                    serverStream.Close();
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
                Window.Game = game;
                Window.UpdateGame();
                
                

                // and again wait for game data
                this.WaitForDataReceive();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                Window.PutLogMessage("ERROR: " + ex);
            }

            
        }

    }
}
