using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public Action ActionMade
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

        public void Add(Player player)
        {
            int i = 0;
            while (this.Players[i] != null)
                i++;
            this.Players[i] = player;
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
                    


        /*
        public override string ToString()
        {
            //return this.Name;
        }
        */

    }

    [Serializable]
    public class PlayersCollection
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
    }

    [Serializable]
    public class Player
    {
        public string Name
        {
            get;
            set;
        }

        public int Ammount
        {
            get;
            private set;
        }
        private int raiseValue;

        public Player(string name, int ammount)
        {
            this.Name = name;
            this.Ammount = ammount;
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
            

    }

    [Serializable]
    public enum Action
    {
        NONE,
        CHECK,
        FOLD,
        RAISE,
    }


}
