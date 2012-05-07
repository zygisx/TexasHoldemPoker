using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommonClassLibrary;

namespace Poker.Logic
{
    public class Player : IComparable
    {

        private PokerClient client;
        private ClientsGame game;



        public string Name
        {
            get
            {
                return this.client.Name;
            }
            private set
            {
                //this.client.Name = value;
            }
        }

        public int Amount
        {
            get;
            set;
        }

        public int Seat  //which seat is taken 0..5 value
        {
            get;
            set;
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


        public PokerClient Client
        {
            get
            {
                return this.client;
            }
            private set
            {
            }
        }

        public ClientsGame Game
        {
            get
            {
                return this.game;
            }
            private set
            {
            }
        }


        public Player(PokerClient client, int ammount, int seat)
        {
            this.client = client;
            this.Amount = ammount;
            this.Seat = seat;
        }

        public void Reduce(int amount)
        {
            if (amount <= this.Amount)
            {
                this.Amount -= amount;
            }
        }

        public int CompareTo(object o)
        {
            if (o is Player)
                return ((Player)o).Seat.CompareTo(this.Seat);
            throw new ArgumentException("Object is not Player");
        }

        public static bool operator <(Player a, Player b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return false;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Seat < b.Seat;
        }

        public static bool operator >(Player a, Player b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return false;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Seat > b.Seat;
        }
    }
}
