using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonClassLibrary
{
    [Serializable]
    public enum Suit { 
        None, 
        Diamonds, 
        Hearts, 
        Clubs, 
        Spades,
    }
    [Serializable]
    public enum Rank {
        None,
        Ace, 
        Two, 
        Three, 
        Four, 
        Five, 
        Six, 
        Seven, 
        Eight,
        Nine, 
        Ten, 
        Jack, 
        Queen, 
        King,
    }
    [Serializable]
    public class Card : IComparable
    {
        private Rank _rank;
        private Suit _suit;

        public Rank rank
        {
            get { return _rank; }
        }

        public Suit suit
        {
            get { return _suit; }
        }

        public Card(Rank rank, Suit suit)
        {
            this._rank = rank;
            this._suit = suit;
        }
        public Card()
            : this(Rank.None, Suit.None)
        {
        }

        // IComparable interface method
        public int CompareTo(object o)
        {
            if (o is Card)
            {
                Card c = (Card)o;
                if (_rank < c.rank)
                    return -1;
                else if (_rank > c.rank)
                    return 1;
                return 0;
            }
            throw new ArgumentException("Object is not a Card");
        }

        public override string ToString()
        {
            if (this._suit == Suit.None) return "NN";
            return this._suit.ToString().ElementAt(0) +  this._rank.ToString();
        }
    }
}
