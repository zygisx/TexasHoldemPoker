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
using System.Windows.Navigation;
using System.Windows.Shapes;

using CommonClassLibrary;

namespace PokerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            Client client = new Client();
            if (String.IsNullOrEmpty(nickTextBox.Text))
            {
                this.errorLabel.Content = "Choose nickname.";
            }

            if (!client.ConnectToServer(ipTextBox.Text, Convert.ToInt32(portTextBox.Text), nickTextBox.Text))
            {
                switch (client.ConnectionState)
                {
                    case State.NICK_USED:
                        this.FailedToConnect();
                        
                        return;
                    case State.ROOM_FULL:
                        this.RoomFull();
                        return;
                }
            }
            new GameWindow(this, client).Show();
            this.Hide();
        }

        private void RoomFull()
        {
            this.errorLabel.Content = "Room is full. Try later.";
        }

        private void FailedToConnect()
        {
            this.errorLabel.Content = "Nick is already in use. Chose another one.";
        }
    }
}
