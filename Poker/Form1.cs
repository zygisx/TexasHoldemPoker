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

using CommonClassLibrary;
using Poker.Logic;




namespace Poker
{
    public partial class Form1 : Form
    {
        const int START_AMMOUNT = 200;

        private Thread serverThread;
        private Thread onlinePlayersCheckThread;
        private TcpListener listener;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private bool isServerRunning = true;
        private bool isDisconnected = false;

        //Logic related
        //private List<PokerClient> queue;
        
        // 
        public Game PokerGame
        {
            get;
            set;
        }
        /*
        public ClientsGame CGame
        {
            get;
            set;
        }
        */
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
            this.PokerGame = new Game();
        }

        private void StartServer(object sender, EventArgs e)
        {
            this.startButton.Enabled = false;
            serverThread = new Thread(new ThreadStart(RunServer));
            serverThread.Start();
        }

        public void PutLogMessage(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new MethodInvoker(delegate { logTextBox.AppendText(message + "\n"); }));
            }
            else logTextBox.AppendText(message + "\n");

        }

        public void UpdatePlayerList()
        {
            ClearPlayersList();
            foreach (Logic.Player p in this.PokerGame)
            {
                this.AppendPlayer(p.Name);
            }
        }

        public void AppendPlayer(string message)
        {
            if (playersTextBox.InvokeRequired)
            {
                playersTextBox.Invoke(new MethodInvoker(delegate { playersTextBox.AppendText(message + "\n"); }));
            }
            else playersTextBox.AppendText(message + "\n");
        }

        public void ClearPlayersList()
        {
            if (playersTextBox.InvokeRequired)
            {
                playersTextBox.Invoke(new MethodInvoker(delegate { this.playersTextBox.Clear(); }));
            }
            else this.playersTextBox.Clear();
        }


        public void RunServer() {

            IPAddress localIPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipLocal = new IPEndPoint(localIPAddress, 8888);

            listener = new TcpListener(ipLocal);
            listener.Start();
            PutLogMessage("Server started");
            
            // separate thread for waiting clients
            WaitForClientConnect();

            // separate thread for checking if players disconected
            //this.onlinePlayersCheckThread = new Thread(new ThreadStart(CheckPlayers));
            //this.onlinePlayersCheckThread.Start();


            //this.Game = new ClientsGame();

            while (this.isServerRunning)
            {
                if (this.PokerGame.PlayersCount == 0)
                {
                    //PutLogMessage("0 users");
                    Thread.Sleep(500);
                    continue;
                }
                if (this.PokerGame.State == GameState.WAITING_PLAYERS)
                {
                    if (this.PokerGame.ActivePlayersCount >= 2)
                    {
                        this.PokerGame.NewPlay();
                        this.SendGameToAll();
                        //continue;
                    }
                    else
                    {
                        this.PokerGame.GameInWaitingStateWork();
                        this.SendGameToAll();
                        Thread.Sleep(500);
                        continue;
                    }
                }
                

                //TODO change
                allDone.Reset();
                this.isDisconnected = false;

                PokerClient client = this.PokerGame.GetQueueTop();
                
                client.WaitForRequest();

                if (! this.isDisconnected && ! this.PokerGame.checkAndResetIsFolded())
                {
                    this.PokerGame.MoveTopToBack();
                }
                this.CheckPayers();
                this.SendGameToAll();

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
                PokerClient client = new PokerClient(this, clientSocket);
                if (!client.StartClient())
                {
                    client.SendStateObject();
                    WaitForClientConnect();
                    return;
                }
                client.SendStateObject();
                this.PokerGame.Add(client, START_AMMOUNT);
                PutLogMessage(client.Name + " connected");

                CheckPayers();
                UpdatePlayerList();
                this.SendGameToAll();
                //client.SendData("Connected succesfully");
            }
            catch (Exception se)
            {
                PutLogMessage(se.ToString());
            }

            WaitForClientConnect();
        }

        public void SendGameToAll()
        {
            foreach (Logic.Player p in this.PokerGame)
            {
                if (p.Client.IsConnected())
                {
                    p.Client.SendGame(this.PokerGame.GetClientGame(p.Name));
                }
            }
        }

        // check if somebody disconnected
        public void CheckPlayersInfiniteLoop()
        {
            while (true)
            {
                this.CheckPayers();
                Thread.Sleep(5000);
            }
        }


        public void CheckPayers()
        {
            foreach (Poker.Logic.Player p in this.PokerGame)
            {
                if (!p.Client.IsConnected()) this.Disconnected(p.Client);
            }
        }

        public void Disconnected(PokerClient client)
        {
            if (! this.PokerGame.isQueueEmpty() && client.Name == this.PokerGame.GetQueueTop().Name)
                this.isDisconnected = true;
            PutLogMessage(client.Name + " disconnected");
            this.PokerGame.Remove(client);

            client.Disconnect();
            //Game.Remove(client.Name);
            UpdatePlayerList();
            
        }

        public void ProcessGameData(GameAction action, string name)
        {
            
            this.PokerGame.ResetClientGame();
            this.PokerGame.SetGameAction(action);
            switch (action.GameAct)
            {
                case CommonClassLibrary.Action.FOLD:
                    this.PokerGame.PlayerFold(name);
                    break;
                case CommonClassLibrary.Action.RAISE:
                    this.PokerGame.PlayerRaised(name, action.Raised);
                    break;
                case CommonClassLibrary.Action.CHECK:
                    this.PokerGame.PlayerCheckCall(name);
                    break;

            }
           
            this.PokerGame.GameProgressWork(name);
        }


        /* on form closing */
        private void StopServer(object sender, FormClosingEventArgs e)
        {
            this.AllDone.Set();
            this.isServerRunning = false;
            this.PokerGame.DisconnectAll();
        }
    }
}
