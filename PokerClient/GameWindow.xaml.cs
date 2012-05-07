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
        public const int CARDS_ON_TABLE = 5;

        private Client client;
        private MainWindow parentWindow;
        private PlayersComponents[] components;
        private Image[] tableCardsImages;
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
            //
            this.createCardsArray();

            this.nameLabel.Content = this.client.Name + " table";
            
            
            client.WaitForDataReceive();
            // Game = new ClientsGame();
        }

        
        public void UpdateGame() {

            // invoke to acces ui from  non ui thread
            this.Dispatcher.BeginInvoke( (System.Action)delegate()
            {
                // update player labels 
                this.updatePlayerLabels();
                this.markActivePlayer();
                this.updateCards();

                // enbable buttons
                this.raiseButton.Visibility = System.Windows.Visibility.Visible;


                this.potLabel.Content = this.Game.Pot.ToString();
                this.infoLabel.Content = this.Game.InfoText;
                
                this.addLogMessage();

                // if active player is current player
                if (this.Game.ActivePlayer >= 0 &&
                    this.Game.Players[this.Game.ActivePlayer].Name == this.client.Name ) 
                {
                    this.updateCurrentPlayersWindow();
                }
                else 
				{
					this.callInfoLabel.Content = "";
                    this.slider.Visibility = System.Windows.Visibility.Hidden;
                    this.sliderLbl.Visibility = System.Windows.Visibility.Hidden;
				}

              
            });
        }

       

        public void PutLogMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            if (logText.Dispatcher.CheckAccess())
            {
                // The calling thread owns the dispatcher, and hence the UI element
                //logText.AppendText(text + "\n");
                logText.Text += text + "\n";
            }
            else
            {
                // Invokation required
                logText.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate() { logText.Text += text + "\n"; ; });
            }
        }

        /* Buttons */
		private void Check(object sender, System.Windows.RoutedEventArgs e)
        {
            /*
        	Game.ActionMade = CommonClassLibrary.Action.CHECK;
            this.client.SendGame();
            */
            this.client.SendGame(new GameAction(
                this.client.Name,
                CommonClassLibrary.Action.CHECK)
            );
        }

        private void Raise(object sender, System.Windows.RoutedEventArgs e)
        {
            /*
            Game.ActionMade = CommonClassLibrary.Action.RAISE;
            Game.RaiseValue = 10; //FIXME
            this.client.SendGame();
             */
            GameAction action = new GameAction(this.client.Name, CommonClassLibrary.Action.RAISE);
            action.Raised = Convert.ToInt32(this.slider.Value); //FIXME
            this.client.SendGame(action);
        }
		
		private void Fold(object sender, System.Windows.RoutedEventArgs e)
        {
            //Game.ActionMade = CommonClassLibrary.Action.FOLD;

            //this.client.SendGame();

            this.client.SendGame(new GameAction(
                this.client.Name,
                CommonClassLibrary.Action.FOLD)
            );
        }

        public void Disconnect() {
            this.client.CloseConnection();
            //this.Hide();
            this.parentWindow.Show();
        }
 
        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.client.CloseConnection();
            //this.parentWindow.Show();
            //this.Disconnect();
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
            this.components[2].Card1 = image5;
            this.components[2].Card2 = image6;
            this.components[3].Card1 = image7;
            this.components[3].Card2 = image8;
            this.components[4].Card1 = image9;
            this.components[4].Card2 = image10;
            this.components[5].Card1 = image11;
            this.components[5].Card2 = image12;

            this.components[0].Rect = rect1;
            this.components[1].Rect = rect2;
            this.components[2].Rect = rect3;
            this.components[3].Rect = rect4;
            this.components[4].Rect = rect5;
            this.components[5].Rect = rect6;

        }

        private void createCardsArray()
        {
            this.tableCardsImages = new Image[CARDS_ON_TABLE];
            this.tableCardsImages[0] = this.tableCard1;
            this.tableCardsImages[1] = this.tableCard2;
            this.tableCardsImages[2] = this.tableCard3;
            this.tableCardsImages[3] = this.tableCard4;
            this.tableCardsImages[4] = this.tableCard5;
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
                    this.components[i].Rect.Visibility = System.Windows.Visibility.Hidden;
                    this.components[i].Cashlbl.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    name = Game.Players[i].Name;
                    cash = Game.Players[i].Amount.ToString();
                    this.components[i].Rect.Visibility = System.Windows.Visibility.Visible;
                    this.components[i].Cashlbl.Visibility = System.Windows.Visibility.Visible;

                    // check if playing in current play
                    if (Game.Players[i].Card1 == null)
                        this.components[i].Rect.StrokeThickness = 2;
                    else
                        this.components[i].Rect.StrokeThickness = 5;
                }

                this.components[i].Namelbl.Content = name;

                this.components[i].Rect.Fill = Brushes.Transparent;

                this.components[i].Cashlbl.Content = "Cash: " + cash + "$";
            }
        }

        private void markActivePlayer()
        {
            if (Game.ActivePlayer != -1 && Game.Players[Game.ActivePlayer] != null)
            {
                this.components[Game.ActivePlayer].Rect.Fill = Brushes.SeaGreen;
            }
        }

        private void updateCards()
        {
            // players
            for (int i = 0; i < PLAYERS; i++)
            {
                if (this.Game.Players[i] == null)
                {
                    this.components[i].Card1.Visibility = System.Windows.Visibility.Hidden;
                    this.components[i].Card2.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    if (this.Game.Players[i].Card1 != null)
                    {
                        this.components[i].Card1.Visibility = System.Windows.Visibility.Visible;
                        this.components[i].Card1.Source = this.Resources[this.Game.Players[i].Card1.ToString()] as ImageSource;
                        //this.PutLogMessage("nullas");
                    }
                    else this.components[i].Card1.Visibility = System.Windows.Visibility.Hidden; // palyer is connected but not playing in current play
                    if (this.Game.Players[i].Card2 != null)
                    {
                        this.components[i].Card2.Visibility = System.Windows.Visibility.Visible;
                        this.components[i].Card2.Source = this.Resources[this.Game.Players[i].Card2.ToString()] as ImageSource;
                    }
                    else this.components[i].Card2.Visibility = System.Windows.Visibility.Hidden;
                }
            }

            // table
            int j = 0;
            while (j < CARDS_ON_TABLE && this.Game.PokerTable.TableCards[j] != null)
            {
                this.tableCardsImages[j].Visibility = System.Windows.Visibility.Visible;
                this.tableCardsImages[j].Source = this.Resources[this.Game.PokerTable.TableCards[j].ToString()] as ImageSource;
                j++;
            }
            j = 0;
            while (j < CARDS_ON_TABLE && this.Game.PokerTable.TableCards[j] == null)
            {
                this.tableCardsImages[j].Visibility = System.Windows.Visibility.Hidden;
                j++;
            }
        }

        private void updateCurrentPlayersWindow()
        {
            this.raiseButton.Visibility = System.Windows.Visibility.Visible;

            // if player has to call
            if (this.Game.Players[this.Game.ActivePlayer].AmountOnTable < this.Game.Raised)
            {
                int valueToCall = this.Game.Raised - this.Game.Players[this.Game.ActivePlayer].AmountOnTable;

                //if palyer has enaugh money to call
                if (valueToCall < this.Game.Players[this.Game.ActivePlayer].Amount)
                {
                    this.callInfoLabel.Content = "Value to call: " + valueToCall;
                    this.slider.Visibility = System.Windows.Visibility.Visible;
                    this.slider.Maximum = this.Game.Players[this.Game.ActivePlayer].Amount - valueToCall;
                    this.slider.Minimum = 10;

                    int minAmount = this.Game.GetMaxRaiseValue(this.Game.Raised);
                    if (minAmount < this.slider.Maximum)
                    {
                        this.slider.Maximum = minAmount;
                    }


                    this.slider.Value = 10; //TODO need const
                    if (minAmount < 10)
                    {
                        this.slider.Visibility = System.Windows.Visibility.Hidden;
                        this.raiseButton.Visibility = System.Windows.Visibility.Hidden;
                    }
                    else
                    {
                        this.sliderLbl.Visibility = System.Windows.Visibility.Visible;
                        this.sliderLbl.Content = "Raise: " + Convert.ToInt32(this.slider.Value).ToString() + "$";
                    }
                }
                //else all in  
                else
                {
                    this.raiseButton.Visibility = System.Windows.Visibility.Hidden;
                    this.callInfoLabel.Content = "Value to call: " + this.Game.Players[this.Game.ActivePlayer].Amount;

                    //this.slider.Visibility = System.Windows.Visibility.Hidden;
                    //this.sliderLbl.Visibility = System.Windows.Visibility.Visible;
                    //this.sliderLbl.Content = "Raise: " + Convert.ToInt32(this.slider.Value).ToString() + "$";
                    // no slider showing

                    // only call all in is posible
                }
            }
            // player has to check or rise
            else
            {
                this.slider.Visibility = System.Windows.Visibility.Visible;

                this.slider.Maximum = this.Game.Players[this.Game.ActivePlayer].Amount;
                int minAmount = this.Game.GetMinimumValue();
                
                if (minAmount < this.slider.Maximum)
                {
                    this.slider.Maximum = minAmount;
                }

                if (minAmount < 10)
                {
                    this.slider.Visibility = System.Windows.Visibility.Hidden;
                    this.raiseButton.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    this.sliderLbl.Visibility = System.Windows.Visibility.Visible;
                    this.sliderLbl.Content = "Raise: " + Convert.ToInt32(this.slider.Value).ToString() + "$";
                }
            }
        }

        private void SliderValueChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                this.sliderLbl.Content = "Raise: " + Convert.ToInt32(e.NewValue).ToString() + "$";
            }
            catch (Exception) { };
        }
       
        #region Code for log message
        // field used only for log messages
        private bool isWaitingForPlayers = false;

        private void addLogMessage() {
            if (this.Game.ActivePlayer >= 0 && this.Game.ActionMade != null)
            {
                this.PutLogMessage(this.Game.ActionMade.Name + ": "
                    + this.Game.ActionMade.GameAct.ToString() + " "
                    + ((this.Game.ActionMade.GameAct == CommonClassLibrary.Action.RAISE)
                    ?
                    this.Game.ActionMade.Raised + "$." : "."));
                isWaitingForPlayers = false;
            }
            else
            {
                if (!isWaitingForPlayers)
                {
                    this.PutLogMessage("Waiting for players...");
                    isWaitingForPlayers = true;
                }
            }
            this.logText.ScrollToEnd();
        }
        #endregion
        
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

        public Rectangle Rect
        {
            get;
            set;
        }
    }


}
