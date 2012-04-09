using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonClassLibrary
{
    [Serializable]
    public enum Suit { 
        NONE, 
        DIAMONDS, 
        HEARTS, 
        CLUBS, 
        SPADES,
    }
    [Serializable]
    public enum Rank {
        NONE,
        ACE, 
        TWO, 
        THREE, 
        FOUR, 
        FIVE, 
        SIX, 
        SEVEN, 
        EIGHT,
        NINE, 
        TEN, 
        JACK, 
        QUEEN, 
        KING
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
            : this(Rank.NONE, Suit.NONE)
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
            return this._rank + " " + this._suit;
        }
    }
}
