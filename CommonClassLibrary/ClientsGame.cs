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

        public ClientsGame()
        {
            players = new PlayersCollection();
            for (int i = 0; i < 8; i++)
                Players[i] = null;
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
                if (p != null)
                    p.ReverseCards();
            }
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



        private int raiseValue;

        public Player(string name, int ammount)
        {
            this.Name = name;
            this.Ammount = ammount;
            this.Card1 = new Card();
            this.Card2 = new Card();
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

        public bool SetRaiseValue(int ammount)
        {
            if (this.raiseValue <= ammount)
            {
                this.raiseValue = ammount;
                return true;
            }
            else return false;
        }

        public void ReverseCards()
        {
            this.Card1 = new Card();
            this.Card2 = new Card();
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
