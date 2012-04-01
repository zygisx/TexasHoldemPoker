using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace PokerServer
{
    public partial class Form1 : Form
    {
        private Thread serverThread;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private static TcpListener listener;
        private static List<ClientCommunication> queue;

        /* properties */
        public ManualResetEvent AllDone
        {
            get
            {
                return allDone;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void ShutDownServer(object sender, EventArgs e)
        {

        }

        private void StartServer(object sender, EventArgs e)
        {
            this.button2.Enabled = false;

            // Run server in other thread
            serverThread = new Thread(new ThreadStart(RunServer));
            serverThread.Start();
        }

        public void PutLogMessage(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new MethodInvoker(delegate { logTextBox.AppendText(message + "\n"); }));
            }
        }

        public void  UpdatePlayersList() 
        {

        }


        // Network 
        public void RunServer()
        {
            //todo check IP
            IPAddress localIPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipLocal = new IPEndPoint(localIPAddress, 8000);

            listener = new TcpListener(ipLocal);
            listener.Start();
            PutLogMessage("Server started");
            WaitForClientConnect();
            //object obj = new object();
            //OnClientConnect(obj);

            queue = new List<ClientCommunication>(7);
            while (true)
            {
                if (queue.Count == 0)
                {
                    Thread.Sleep(1000);
                    //parent.PutMessage("0 users");
                    continue;
                }
                //todo change
                allDone.Reset();
                int count = queue.Count;
                ClientCommunication client = queue.ElementAt(0);
                //queue.RemoveAt(0);
                client.WaitForRequest();
                if (count == queue.Count)
                {
                    queue.RemoveAt(0);
                    queue.Add(client);
                }

            }

        }

        public void WaitForClientConnect()
        {
            object obj = new object();
            listener.BeginAcceptTcpClient(new System.AsyncCallback(OnClientConnect), obj);

        }

        private void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                TcpClient clientSocket = default(TcpClient);
                clientSocket = listener.EndAcceptTcpClient(asyn);
                ClientCommunication client = new ClientCommunication(this, clientSocket);

                client.StartClient();

                queue.Add(client);

                PutLogMessage(client.Name + " connected");
            }
            catch (Exception se)
            {
                PutLogMessage(se.ToString());
            }

            WaitForClientConnect();
        }

        public void Disconnected(ClientCommunication client)
        {
            //PutLogMessage("Was: " + queue.Count);
            queue.Remove(client);
            //PutLogMessage("IS: " + queue.Count);
        }
    }

    public class ClientCommunication
    {
        private TcpClient clientSocket;
        private NetworkStream networkStream = null;
        private Form1 main;
        
        /* Proerties */
        public string Name 
        {
            get;
            private set;
        }

        public ClientCommunication(Form1 frame, TcpClient client)
        {
            this.main = frame;
            this.clientSocket = client;

        }

        public void StartClient()
        {
            networkStream = clientSocket.GetStream();
            // WaitForRequest();


            // get name
            byte[] buffer = new byte[25];
            int read = networkStream.Read(buffer, 0, 25);
            string data = Encoding.Default.GetString(buffer, 0, read);
            this.Name = data;
        }

        public void WaitForRequest()
        {
            if (! IsConnected())
            {
                main.PutLogMessage(this.Name + " disconnected");
                main.Disconnected(this);
                return;
            }

            byte[] buffer = new byte[clientSocket.ReceiveBufferSize];


            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
            main.AllDone.WaitOne();
        }

        private void ReadCallback(IAsyncResult result)
        {


            NetworkStream networkStream = clientSocket.GetStream();
            try
            {
                int read = networkStream.EndRead(result);
                if (read == 0)
                {
                    networkStream.Close();
                    clientSocket.Close();
                    return;
                }

                byte[] buffer = result.AsyncState as byte[];
                string data = Encoding.Default.GetString(buffer, 0, read);

                //do the job with the data here
                //send the data back to  [ALL] client.
                Byte[] sendBytes = Encoding.ASCII.GetBytes(this.Name + " Processed " + data);
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();

                main.PutLogMessage(this.Name + " Received: " + data);

                main.AllDone.Set();
            }
            catch (Exception ex)
            {
                main.PutLogMessage(this.Name + " disconnectedddd");
                main.Disconnected(this);
                main.AllDone.Set();
            }

            //this.WaitForRequest();
        }

        public bool IsConnected()
        {
            try
            {
                return !(this.clientSocket.Client.Poll(1, SelectMode.SelectRead) && this.clientSocket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

    }
}
