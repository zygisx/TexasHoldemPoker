using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CommonClassLibrary
{
    [Serializable]
    public class ClientsGame
    {

        private PlayersCollection players;
        
        public PlayersCollection Players
        {
            get
            {
                return players;
            }
            set 
            {
                players = value;
            }
        }

        public int CurrentPlayer
        {
            get;
            set;
        }

        public Action ActionMade
        {
            get;
            set;
        }

        public int ActivePlayer
        {
            get;
            set;
        }

        public Table PokerTable
        {
            get;
            private set;
        }

        public string InfoText
        {
            get;
            set;
        }

        public int RaiseValue
        {
            get;
            private set;
        }

        public ClientsGame()
        {
            players = new PlayersCollection();
            for (int i = 0; i < 8; i++)
                Players[i] = null;
            this.PokerTable = new Table();
        }

        public void Add(Player player, int seat)
        {
            this.Players[seat] = player;
        }

        public void Remove(string name)
        {
            int i = 0;
            while (i < 8)
            {
                if (players[i] != null &&
                    players[i].Name == name)
                {
                    players[i] = null;
                    break;
                }
                i++;
            }
        }

        public void ReverseAllCards()
        {
            foreach (Player p in this.players)
            {
                if ( p != null && p.Card1 != null && p.Card2 != null)
                    p.ReverseCards();
            }
        }

        public void PlayerFold(int playerSeat)
        {
            this.players[playerSeat].Card1 = null;
            this.players[playerSeat].Card2 = null;
        }

        public bool SetRaiseValue(int ammount)
        {
            if (this.RaiseValue <= ammount)
            {
                this.RaiseValue = ammount;
                return true;
            }
            else return false;
        }
                    


        /*
        public override string ToString()
        {
            //return this.Name;
        }
        */

    }

    [Serializable]
    public class PlayersCollection : IEnumerable
    {
        private Player[] players = new Player[8];


        public Player this[int index]
        {
            get
            {
                return players[index];
            }
            set
            {
                players[index] = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.players.GetEnumerator();
        }
    }

    [Serializable]
    public class Player
    {
        public string Name
        {
            get;
            private set;
        }

        public int Ammount
        {
            get;
            private set;
        }

        public Card Card1
        {
            get;
            set;
        }

        public Card Card2
        {
            get;
            set;
        }

        

        public Player(string name, int ammount)
        {
            this.Name = name;
            this.Ammount = ammount;
            this.Card1 = null;
            this.Card2 = null;
        }

        public bool Reduce(int ammount)
        {
            if (this.Ammount >= ammount)
            {
                this.Ammount -= ammount;
                return true;
            }
            else return false;
        }

        public bool Increase(int ammount)
        {
            this.Ammount += ammount;
            // return value bool just in case if in future some restrictions will be added
            return true;
        }
        /*
        public bool SetRaiseValue(int ammount)
        {
            if (this.RaiseValue <= ammount)
            {
                this.RaiseValue = ammount;
                return true;
            }
            else return false;
        }
        */
        public void ReverseCards()
        {
            this.Card1 = new Card();
            this.Card2 = new Card();
        }
            

    }

    [Serializable]
    public class Table
    {
        public const int TABLE_CARDS = 5;

        private TableCards cards = new TableCards();

        public TableCards TableCards
        {
            get
            {
                return this.cards;
            }
            private set
            {
                this.cards = value;
            }
        }


        public Table()
        {
            this.Reset();
        }

        public void Flop(Card c1, Card c2, Card c3)
        {
            this.cards[0] = c1;
            this.cards[1] = c2;
            this.cards[2] = c3;
        }

        public void Fourth(Card c)
        {
            this.cards[3] = c;
        }

        public void Fifth(Card c)
        {
            this.cards[4] = c;
        }



        public void Reset()
        {
            for (int i = 0; i < TABLE_CARDS; i++)
            {
                this.cards[i] = null;
            }
        }
    }

    [Serializable]
    public class TableCards : IEnumerable
    {
        private Card[] cards = new Card[Table.TABLE_CARDS];


        public Card this[int index]
        {
            get
            {
                return this.cards[index];
            }
            set
            {
                this.cards[index] = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.cards.GetEnumerator();
        }
    }

    [Serializable]
    public enum Action
    {
        NONE,
        CHECK,
        FOLD,
        RAISE,
    }
    [Serializable]
    public enum State
    {
        OK,
        NICK_USED,
        ROOM_FULL,
    }


}
