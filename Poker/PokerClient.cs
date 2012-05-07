using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using CommonClassLibrary;

namespace Poker
{
    public class PokerClient
    {
        private TcpClient clientSocket;
        //private NetworkStream networkStream;
        private Form1 main;

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

        public PokerClient(Form1 frame, TcpClient client)
        {
            this.main = frame;
            this.clientSocket = client;
        }

        public bool StartClient()
        {
            NetworkStream networkStream = clientSocket.GetStream();

            // get name
            byte[] buffer = new byte[32];
            int read = networkStream.Read(buffer, 0, 32);
            string data = Encoding.Default.GetString(buffer, 0, read);
            if (!main.PokerGame.isNameFree(data))
            {
                this.ConnectionState = State.NICK_USED;
                return false;
            } //TODO add const
            else if (main.PokerGame.PlayersCount > 5)
            {
                this.ConnectionState = State.ROOM_FULL;
                return false;
            }

            this.ConnectionState = State.OK;
            this.Name = data;
            return true;
        }

        public void WaitForRequest()
        {
            if ((this.clientSocket == null) || (! this.clientSocket.Connected) || (! this.IsConnected()))
            {
                main.Disconnected(this);
                return;
            }
            NetworkStream networkStream = clientSocket.GetStream();

            // Avoid data already in stream
            if (networkStream.CanRead)
            {
                byte[] myReadBuffer = new byte[1028];
                // Incoming message may be larger than the buffer size.
                while (networkStream.DataAvailable)
                {
                    networkStream.Read(myReadBuffer, 0, myReadBuffer.Length);
                }
            }
            byte[] buffer = new byte[2048];

            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
            main.AllDone.WaitOne();
        }

        private void ReadCallback(IAsyncResult result)
        {


            NetworkStream networkStream = clientSocket.GetStream();
            MemoryStream memStream = new MemoryStream();
            try
            {
                int read = networkStream.EndRead(result);
                if (read == 0)
                {
                    networkStream.Close();
                    clientSocket.Close();
                    main.Disconnected(this);
                    return;
                }
                
                byte[] buffer = result.AsyncState as byte[];
                memStream.Write(buffer, 0, read);
                memStream.Position = 0;

                BinaryFormatter bf = new BinaryFormatter();
                GameAction game = (GameAction)bf.Deserialize(memStream);
                
                // process data received 
                main.ProcessGameData(game, this.Name);
                main.PutLogMessage(this.Name + " received: " + game.GameAct);

                // and send data back to all clients
                //main.SendGameToAll();

            }
            catch (Exception ex)
            {
                main.Disconnected(this);
            }
            finally
            {
                main.AllDone.Set();
            }
            //this.WaitForRequest();

        }

        public void SendData(string data)
        {

            NetworkStream networkStream = clientSocket.GetStream();
            Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
            networkStream.Write(sendBytes, 0, sendBytes.Length);
            networkStream.Flush();

        }

        public void SendGame(ClientsGame game)
        {
            
            
            try
            {
                NetworkStream serverStream = clientSocket.GetStream();
                MemoryStream memStream = new MemoryStream(3072);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(memStream, game);
                
                byte[] buffer = memStream.GetBuffer();
                serverStream.Write(buffer, 0, buffer.Length);
                serverStream.Flush();
             
            }
            catch (Exception ex)
            {
                main.Disconnected(this);
                main.PutLogMessage("ERRRRRRR" + ex.ToString());
            }
        }

        public void SendStateObject()
        {
            this.SendObject(this.ConnectionState);
        }

        public void SendObject(object o)
        {
            NetworkStream serverStream = clientSocket.GetStream();
            MemoryStream memStream = new MemoryStream();
            try
            {

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(memStream, o);

                byte[] buffer = memStream.GetBuffer();
                serverStream.Write(buffer, 0, buffer.Length);
                serverStream.Flush();

            }
            catch (Exception ex)
            {
                main.PutLogMessage(ex.ToString());
            }
        }


        public bool IsConnected()
        {
            try
            {
                if (this.clientSocket != null)
                    return !(this.clientSocket.Client.Poll(1, SelectMode.SelectRead) && this.clientSocket.Available == 0);
                else return false;
            }
            catch (SocketException) { return false; }
        }

        public void Disconnect()
        {
            //this.clientSocket.GetStream().Close();   // don't know why this line makes program crash
            this.clientSocket.Close();
        }
    }
}
