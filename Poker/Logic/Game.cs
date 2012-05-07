using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommonClassLibrary;
using System.Collections;

namespace Poker.Logic
{
    public class Game : IEnumerable
    {
        public const int PLAYERS = 6;
        public const int SMALL_BLIND = 10;
        public const int BIG_BLIND = 20;

        private List<Player> queue;
        private SortedDictionary<string, Player> players;
        private GameState state;
        private GameStage stage;

        private int pot;
        //private Player[] players;
        private ClientsGame clientGame;
        private Deck deck;
        private string circleStartPlayer = null;
        private bool isFolded = false;
        private bool flagForFoldedPlayer = false;
        
        private string smallName; //player has big blind


        public int ActivePlayersCount
        {
            get
            {
                int count = 0;
                foreach (Player p in this.players.Values.ToList())
                {
                    if (p.Amount > 10) count++;
                }
                return count;
            }
            private set
            {
            }
        }

        public int PlayersCount
        {
            get
            {
                return this.players.Count;
            }
            private set
            {
            }
        }

        public GameState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        public CommonClassLibrary.Table PokerTable
        {
            get
            {
                return this.clientGame.PokerTable;
            }
        }

        public Game()
        {
            this.deck = new Deck();
            this.queue = new List<Player>();
            this.players = new SortedDictionary<string, Player>();
            this.clientGame = new ClientsGame();
            this.State = GameState.WAITING_PLAYERS;
        }

        public void NewPlay()
        {
            this.State = GameState.GAME_ON;
            this.deck = new Deck();
            this.deck.Shuffle();
            this.PokerTable.Reset();

            this.stage = GameStage.FIRST;
            this.pot = 0;
            this.clientGame.Pot = 0;
            this.clientGame.NewStageReset();
            // in future change how players are queued

            //FIXME for blinds
            //this.smallName = this.NextPlayer();
            this.createQueue();
            this.circleStartPlayer = queue.ElementAt(0).Name;

            this.Deal();
            //this.clientGame.NewStageReset();
            
        }

        

        public PokerClient GetQueueTop()
        {
            if (queue.Count > 0)
                return queue.ElementAt(0).Client;
            return null;
            //TODO throw wxception
        }

        public bool isQueueEmpty()
        {
            return (this.queue.Count == 0);
        }

        public void MoveTopToBack()
        {
            
            if (this.queue.Count > 0)
            {
                Player player = this.queue.ElementAt(0);
                this.queue.RemoveAt(0);
                this.queue.Add(player);
            }
            //TODO throw exception
        }

        public void Add(PokerClient player, int ammount)
        {
            //if (this.PlayersCount == 1)

            int seat = findEmptySeat();
            this.players.Add(player.Name, new Poker.Logic.Player(player, ammount, seat));
            
            //this.addToQueue(seat, this.players[player.Name]);
            
            
            this.clientGame.Add(new CommonClassLibrary.Player(player.Name, ammount), seat );
            
            if (this.State == GameState.WAITING_PLAYERS &&
                this.players.Count > 1)
            {
                this.State = GameState.GAME_ON;
                this.NewPlay();
            }
        }

        public void Remove(PokerClient player)
        {
            this.queue.RemoveAll(x => x.Name == player.Name);
            this.players.Remove(player.Name);
            this.clientGame.Remove(player.Name);
            
            // if only one player left than game is stoped
            if (queue.Count < 2)
            {
                this.State = GameState.WAITING_PLAYERS;
                this.queue.Clear();
            }
        }

        public void PlayerFold(string name)
        {
            this.queue.Remove(this.players[name]);
            if (this.queue.Count > 1)
            {
                if (name == this.circleStartPlayer)
                {

                    this.circleStartPlayer = this.queue.First().Name;
                    this.flagForFoldedPlayer = true;
                }
                this.isFolded = true;

                /*
                Player player = this.queue.Last();
                this.queue.RemoveAt(this.queue.Count - 1);
                this.queue.Insert(0, player);
                */
            }



            this.players[name].Card1 = null;
            this.players[name].Card2 = null;

            this.clientGame.PlayerFold(this.players[name].Seat);

            // if only one player left than game is stoped
            if (queue.Count < 2)
            {
                this.players[this.queue.First().Name].Amount += this.pot;
                this.clientGame.Players[this.queue.First().Seat].Increase(this.pot);
                this.State = GameState.WAITING_PLAYERS;
                this.clientGame.InfoText = this.queue.First().Name + " win Pot";
                this.queue.Clear();
            }
        }

        public void PlayerRaised(string name, int value)
        {
            // at first have to call 
            this.PlayerCheckCall(name);
            
            //and then rise
            this.NewCircleStart(name);
            this.pot += value;
            this.clientGame.Pot += value;
            this.players[name].Reduce(value);
            this.clientGame.Players[this.players[name].Seat].Reduce(value);
            this.clientGame.Raised += value;
            
        }

        public void PlayerCheckCall(string name)
        {
            // if call
            if (this.clientGame.Raised > this.clientGame.Players[this.players[name].Seat].AmountOnTable)
            {

                // If player has enaugh money to call
                int amountToCall = this.clientGame.Raised  - this.clientGame.Players[this.players[name].Seat].AmountOnTable;
                if (this.players[name].Amount > amountToCall)
                {
                    //System.Console.WriteLine(this.clientGame.Players[this.players[name].Seat].Amount + " " + this.clientGame.RaiseValue);
                    
                    this.players[name].Reduce(amountToCall);
                    this.clientGame.Players[this.players[name].Seat].Reduce(amountToCall);
                    this.pot += amountToCall;
                    this.clientGame.Pot += amountToCall;
                }
                // if not then all in
                else
                {
                    // do not change statements order
                    this.pot += this.players[name].Amount;
                    this.clientGame.Pot += this.players[name].Amount;
                    this.clientGame.Players[this.players[name].Seat].Reduce(this.players[name].Amount);
                    this.players[name].Reduce(this.players[name].Amount);

                    //TODO add another pot cauze of all in
                }
            }
            // if check
            else
            {
                // for now do nothing
            }     

        }

        public void SetGameAction(GameAction action)
        {
            this.clientGame.ActionMade = action;
        }

        public bool checkAndResetIsFolded()
        {
            bool answer = this.isFolded;
            this.isFolded = false;
            return answer;
        }

        public void NewCircleStart(string name) 
        {
            this.circleStartPlayer = name;
        }

        public void GameProgressWork(string name)
        {

            int playerNo = 1; // player to check is one after current in game queue

            //if player folded and player whos folded is one before circle start player then we have to upgrade game stage
            // else if player not one before CSP then just return from method, cauze it can't maka any progress
            if (this.isFolded)
            {
                if (this.flagForFoldedPlayer)
                {
                    this.flagForFoldedPlayer = false;
                    return;
                }
                if (this.queue.First().Name == this.circleStartPlayer)
                    playerNo = 0;
                else return;
            }


            if (queue.Count > 0)
                System.Console.WriteLine("Start: " + circleStartPlayer + " Top: " + queue.First().Name);

            if (this.queue.Count > 1 && this.circleStartPlayer == this.queue.ElementAt(playerNo).Name)
            {

                switch (this.stage)
                {
                    case GameStage.FIRST:
                        this.stage = GameStage.FLOP;
                        this.PokerTable.Flop(
                            this.deck.GetCard(),
                            this.deck.GetCard(),
                            this.deck.GetCard()
                        );
                        this.NewStageWork();
                        break;

                    case GameStage.FLOP:
                        this.stage = GameStage.TURN;
                        this.PokerTable.Turn(
                            this.deck.GetCard()
                        );
                        this.NewStageWork();
                        break;

                    case GameStage.TURN:
                        this.stage = GameStage.RIVER;
                        this.PokerTable.River(
                            this.deck.GetCard()
                        );
                        this.NewStageWork();
                        break;

                    case GameStage.RIVER:
                        Player winner = this.FindWinner();
                        winner.Amount += this.pot;
                        this.clientGame.Players[winner.Seat].Increase(this.pot);
                        this.clientGame.InfoText = winner.Name + " wins pot.";
                        this.State = GameState.WAITING_PLAYERS;
                        this.queue.Clear();
                       
                        break;
                }
                

            }
        }

        public void NewStageWork()
        {
            this.clientGame.NewStageReset();

            //remake queue
            this.queue.Sort();
            
            //while (this.queue.ElementAt(0).Name != this.smallName)
            //{
            //    this.MoveTopToBack();
            //}


            this.isFolded = true; // fake to avoid dequeue first element in main server loop


            this.circleStartPlayer = this.queue.First().Name;

        }

        public Player FindWinner()

        {
            Card[] hand = new Card[] {
                this.clientGame.PokerTable.TableCards[0],
                this.clientGame.PokerTable.TableCards[1],
                this.clientGame.PokerTable.TableCards[2],
                this.clientGame.PokerTable.TableCards[3],
                this.clientGame.PokerTable.TableCards[4],
                null,
                null,
            };

            SortedDictionary<int, string> values = new SortedDictionary<int, string>();

            foreach (Player p in players.Values)
            {
                if (p.Card1 != null)
                {
                    hand[5] = p.Card1;
                    hand[6] = p.Card2;
                    values.Add(HandEvaluator.GetHandValue(hand), p.Name);
                }
            }

            // if many pots exist.
            return this.players[values.First().Value];
        }

        public Player GetPlayerFromQueue(int index)
        {
            if (index < this.queue.Count)
                return this.queue.ElementAt(index);
            return null;
        }

        public void DisconnectAll()
        {
            foreach (Player p in queue)
            {
                p.Client.Disconnect();
            }
        }

        public ClientsGame GetClientGame(string name)
        {
            if (queue.Count > 0)
                this.clientGame.ActivePlayer = this.queue.ElementAt(0).Seat;  //FIXME may couse bug find better solution
            else this.clientGame.ActivePlayer = -1;


            this.clientGame.ReverseAllCards();
            this.clientGame.Players[this.players[name].Seat].Card1 = this.players[name].Card1;
            this.clientGame.Players[this.players[name].Seat].Card2 = this.players[name].Card2;
            return this.clientGame;
        }

        public void Deal()
        {
            foreach (Player p in queue)
            {
                p.Card1 = this.deck.GetCard();
                p.Card2 = this.deck.GetCard();

                this.clientGame.Players[p.Seat].Card1 = p.Card1;
                this.clientGame.Players[p.Seat].Card2 = p.Card2;
            }
        }

        public void ResetClientGame()
        {
            this.clientGame.InfoText = "";
        }

        public bool isNameFree(string name)
        {
            return !this.players.ContainsKey(name);
        }

        public void GameInWaitingStateWork()
        {
            foreach (Player p in this.players.Values.ToList())
            {
                p.Card1 = null;
                p.Card2 = null;
            }
        }

        private int findEmptySeat()
        {
            SortedSet<int> seats = new SortedSet<int>();
            for (int i = 0; i < PLAYERS; i++) {
                seats.Add(i);
            }
            foreach (Player p in players.Values)
            {
                seats.Remove(p.Seat);
            }
            Random rand = new Random();
            return seats.ElementAt(rand.Next(seats.Count));
                
        }

        private void createQueue()
        {
            SortedSet<Player> playersSet = new SortedSet<Player>(this.players.Values.ToList());
            
            
            


            this.queue.Clear();
            this.queue.AddRange(playersSet);

            // dismiss players whom hasnt got enough money
            foreach (Player p in playersSet)
            {
                if (p.Amount < 10)
                {
                    this.queue.Remove(this.players[p.Name]);


                    this.players[p.Name].Card1 = null;
                    this.players[p.Name].Card2 = null;

                    this.clientGame.PlayerFold(this.players[p.Name].Seat);                   
                }
            }
            // if only one player left than game is stoped
            if (queue.Count < 2)
            {
                this.State = GameState.WAITING_PLAYERS;
                this.queue.Clear();
            }

            //this.PlayerRaised(this.queue.ElementAt(0).Name, SMALL_BLIND);
            //this.MoveTopToBack();
            //this.PlayerRaised(this.queue.ElementAt(0).Name, SMALL_BLIND);
            //this.MoveTopToBack();
        }

        private string NextPlayer()
        {
            if (this.smallName == null || !this.players.ContainsKey(this.smallName))
            {
                this.smallName = this.players.First().Value.Name;
            }
            Player old = this.players[this.smallName];
            Player min, max;
            max = old;
            min = old;
            foreach (Player p in this.players.Values.ToList()) {
                if (p > max) {
                    max = p;
                }
                if (p < min) {
                    min = p;
                }
            }
            if (max.Name != old.Name) return max.Name;
            else return min.Name;
        }

        private void addToQueue(int seat, Player player)
        {
            // find used seat before players seat
            int before = 0;
            int index = 0;
            foreach (Player p in queue)
            {
                if (p.Seat < seat && p.Seat > before)
                {
                    before = p.Seat;
                    index = this.queue.IndexOf(p);
                }
            }
            
            if (index == this.queue.Count)
                this.queue.Add(player);    
            else
                this.queue.Insert(index + 1, player);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.players.Values.ToList().GetEnumerator();
        }
    }
}
