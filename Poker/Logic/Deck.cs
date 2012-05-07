using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommonClassLibrary;

namespace Poker.Logic
{
    class Deck
    {
        public const int DECK_SIZE = 52;

        private Card[] deck;
        private int topCard;
        private Random rand = new Random();

        public Deck()
        {
            this.deck = new Card[52];
            this.topCard = 0;
            int counter = 0;
            
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                    if (r != Rank.None && s != Suit.None)
                        this.deck[counter++] = new Card(r, s);
        }

        public Card GetCard()
        {
            return this.deck[this.topCard++];
        }

        public Card LookAtTopCard() 
        {
            return this.deck[this.topCard];
        }

        public void Shuffle()
        {
            this.Shuffle(8); // optimal time for good shuffle
        }

        // for better shuffling
        public void Shuffle(int times)
        {
            if (times < 1) times = 8;
            for (int i = 0; i < times; i++)
            {
                for (int j = 0; j < DECK_SIZE; j++)
                {
                    int index = rand.Next(DECK_SIZE);
                    swapCards(j, index);
                }
                
            }

        }

        private void swapCards(int i, int j)
        {
            Card tmp = this.deck[i];
            this.deck[i] = this.deck[j];
            this.deck[j] = tmp;
        }


    }
}
