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

        private Client client;
        //private ClientsGame game;

        public ClientsGame Game
        {
            get;
            set;
        }

        public GameWindow (Client client)
        {
            InitializeComponent();
            System.Console.WriteLine(client == null);
            this.client = client;
            this.client.Window = this;
            
            client.WaitForDataReceive();
           // Game = new ClientsGame();
            System.Console.WriteLine("this");
        }

        public void UpdateGame() {
            if (Game.Players[0] != null) SetLabelText(this.player1, Game.Players[0].Name);
            if (Game.Players[1] != null) SetLabelText(this.player2, Game.Players[1].Name);
            if (Game.Players[2] != null) SetLabelText(this.player3, Game.Players[2].Name);
            if (Game.Players[3] != null) SetLabelText(this.player4, Game.Players[3].Name);
        }

        public void SetLabelText(Label lbl, string text)
        {
            lbl.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate() { lbl.Content = text; });
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
 
        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.client.CloseConnection();
        }
    }
}
