using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonClassLibrary;
using PokerCalculator;

namespace Poker.Logic
{
    class HandEvaluator
    {
        // prime numbers 
        private static readonly int[] primes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41 };

        public static int GetHandValue(Card[] cards)
        {

            int[] hand = new int[7];
            for (int i = 0; i < 7; i++)
            {
                hand[i] = CardToInt(cards[i]);
            }
            return PokerLib.eval_7hand(hand);
        }

        
        /*
         * From XPokerEval.CactusKev documentation
         * 
         * 
            This routine initializes the deck.  A deck of cards is
           simply an integer array of length 52 (no jokers).  This
           array is populated with each card, using the following
           scheme:
        
           An integer is made up of four bytes.  The high-order
           bytes are used to hold the rank bit pattern, whereas
           the low-order bytes hold the suit/rank/prime value
           of the card.
        
           +--------+--------+--------+--------+
           |xxxbbbbb|bbbbbbbb|cdhsrrrr|xxpppppp|
           +--------+--------+--------+--------+
        
           p = prime number of rank (deuce=2,trey=3,four=5,five=7,...,ace=41)
           r = rank of card (deuce=0,trey=1,four=2,five=3,...,ace=12)
           cdhs = suit of card
           b = bit turned on depending on rank of card
         * 
         */
        private static int CardToInt(Card c)
        {
            int suit = 0;
            switch(c.suit) {
                case Suit.Clubs:
                    suit = 0x8000;
                    break;
                case Suit.Diamonds:
                    suit = 0x4000;
                    break;
                case Suit.Hearts:
                    suit = 0x2000;
                    break;
                case Suit.Spades:
                    suit = 0x1000;
                    break;
            }
            int rank = RankToInt(c.rank);
            int card = primes[rank] | (rank << 8) | suit | (1 << (16 + rank));

            return card;
        }

        private static int RankToInt(Rank r)
        {
            switch (r)
            {
                case Rank.Two:
                    return 0;
                case Rank.Three:
                    return 1;
                case Rank.Four:
                    return 2;
                case Rank.Five:
                    return 3;
                case Rank.Six:
                    return 4;
                case Rank.Seven:
                    return 5;
                case Rank.Eight:
                    return 6;
                case Rank.Nine:
                    return 7;
                case Rank.Ten:
                    return 8;
                case Rank.Jack:
                    return 9;
                case Rank.Queen:
                    return 10;
                case Rank.King:
                    return 11;
                case Rank.Ace:
                    return 12;
                default:
                    return 0;
            }
        }





    }
}
