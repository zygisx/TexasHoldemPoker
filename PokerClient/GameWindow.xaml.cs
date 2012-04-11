using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using CommonClassLibrary;

namespace PokerClient
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public const int PLAYERS = 6;

        private Client client;
        private MainWindow parentWindow;
        private PlayersComponents[] components;
        //private ClientsGame game;

        public ClientsGame Game
        {
            get;
            set;
        }

        public GameWindow (MainWindow parent, Client client)
        {
            InitializeComponent();
            this.parentWindow = parent;
            this.client = client;
            this.client.Window = this;

            // add components to array
            this.createComponentsArray();
            
            
            client.WaitForDataReceive();
            // Game = new ClientsGame();
        }

        

        public void UpdateGame() {

            // update player labels 
            this.updatePlayerLabels();
            this.markActivePlayer();
            
            this.Dispatcher.BeginInvoke(
            (System.Action)delegate()
            {
                this.image1.Source = this.Resources["DA"] as ImageSource;
            });
            
            

        }

       

        public void PutLogMessage(string text)
        {
            if (logText.Dispatcher.CheckAccess())
            {
                // The calling thread owns the dispatcher, and hence the UI element
                logText.AppendText(text + "\n");
            }
            else
            {
                // Invokation required
                logText.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate() { logText.AppendText(text + "\n"); });
            }
        }

        /* Buttons */
        private void Check(object sender, RoutedEventArgs e)
        {
            Game.ActionMade = CommonClassLibrary.Action.CHECK;
            this.client.SendGame();
          
        }

        private void Fold(object sender, RoutedEventArgs e)
        {
            Game.ActionMade = CommonClassLibrary.Action.FOLD;
            this.client.SendGame();
        }
        private void Raise(object sender, RoutedEventArgs e)
        {
            Game.ActionMade = CommonClassLibrary.Action.RAISE;
            this.client.SendGame();
        }


        public void Disconnect() {
            this.client.CloseConnection();
            this.Hide();
            this.parentWindow.Show();
            }
 
        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.client.CloseConnection();
        }

        private void createComponentsArray()
        {
            this.components = new PlayersComponents[PLAYERS];
            for (int i = 0; i < PLAYERS; i++)
                this.components[i] = new PlayersComponents();
            this.components[0].Namelbl = player1;
            this.components[1].Namelbl = player2;
            this.components[2].Namelbl = player3;
            this.components[3].Namelbl = player4;
            this.components[4].Namelbl = player5;
            this.components[5].Namelbl = player6;

            this.components[0].Cashlbl = playerCash1;
            this.components[1].Cashlbl = playerCash2;
            this.components[2].Cashlbl = playerCash3;
            this.components[3].Cashlbl = playerCash4;
            this.components[4].Cashlbl = playerCash5;
            this.components[5].Cashlbl = playerCash6;

            this.components[0].Card1 = image1;
            this.components[0].Card2 = image2;
            this.components[1].Card1 = image3;
            this.components[1].Card2 = image4;
            
            //TODO add other cards
            //this.components[2].Card1 = image5;
            //this.components[2].Card2 = image6;
            //this.components[3].Card1 = image7;
            //this.components[3].Card2 = image2;
            //this.components[4].Card1 = image1;
            //this.components[4].Card2 = image2;
            //this.components[5].Card1 = image1;
            //this.components[5].Card2 = image2;
            //this.components[6].Card1 = image1;
            //this.components[6].Card2 = image2;

        }


        // Design things
        private void updatePlayerLabels()
        {
            for (int i = 0; i < PLAYERS; i++)
            {
                string name, cash;
                if (Game.Players[i] == null)
                {
                    name = ""; cash = "";
                } else {
                    name = Game.Players[i].Name;
                    cash = Game.Players[i].Ammount.ToString();
                }
                this.components[i].Namelbl.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate()
                {
                    this.components[i].Namelbl.Content = name;
                });
                this.components[i].Namelbl.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate()
                {
                    this.components[i].Namelbl.Background = Brushes.Transparent;
                });
                this.components[i].Cashlbl.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate()
                {
                    this.components[i].Cashlbl.Content = cash;
                });
            }
        }

        private void SetLabelText(Label lbl, string text)
        {
            lbl.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate() { lbl.Content = text; });
        }

        private void markActivePlayer()
        {
            if (Game.ActivePlayer != -1 && Game.Players[Game.ActivePlayer] != null)
            {
                this.components[Game.ActivePlayer].Namelbl.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate()
                {
                    this.components[Game.ActivePlayer].Namelbl.Background = Brushes.RoyalBlue;
                });
            }
        }
                

    }


    class PlayersComponents
    {

        public Label Namelbl
        {
            get;
            set;
        }
        public Label Cashlbl
        {
            get;
            set;
        }

        public Image Card1
        {
            get;
            set;
        }

        public Image Card2
        {
            get;
            set;
        }
    }


}
