using System.Security.AccessControl;
using PokerHandKata.Card;

namespace PokerHandKata
{
    public class HandCalculator
    {
        public Combination CalculateHand(Hand hand)
        {
            if (this.CheckForStraightFlush(hand))
            {
                return Combination.StraightFlush;
            }

            if (this.CheckForFourOfAKind(hand))
            {
                return Combination.FourOfAKind;
            }

            if (this.CheckForFullHouse(hand))
            {
                return Combination.FullHouse;
            }

            if (this.CheckForFlush(hand))
            {
                return Combination.Flush;
            }

            if (this.CheckForStraight(hand))
            {
                return Combination.Straight;
            }

            if (this.CheckForThreeOfAKind(hand))
            {
                return Combination.ThreeOfAKind;
            }

            if (this.CheckForTwoPairs(hand))
            {
                return Combination.TwoPairs;
            }

            if (this.CheckForPair(hand))
            {
                return Combination.Pair;
            }

            return Combination.HighCard;
        }

        private bool CheckForStraightFlush(Hand hand)
        {
            Suit suit = hand.Cards[0].Suit;
            IEnumerable<Card.Card> cardsWithSameSuit = hand.Cards.Where(x => x.Suit == suit);

            if (cardsWithSameSuit.Count() != 5)
            {
                return false;
            }

            IOrderedEnumerable<Card.Card> orderedCards = hand.Cards.OrderBy(x => x.Rank);

            int lastValue = (int)orderedCards.First().Rank - 1;

            foreach (Card.Card card in orderedCards)
            {
                int cardRank = (int)card.Rank;
                if (cardRank != lastValue + 1)
                {
                    return false;
                }
                lastValue = cardRank;
            }
            return true;
        }

        private bool CheckForFourOfAKind(Hand hand)
        {
            Rank rank = hand.Cards[0].Rank;
            IEnumerable<Card.Card> cardsWithSameRank = hand.Cards.Where(x => x.Rank == rank);

            return cardsWithSameRank.Count() == 4;
        }

        private bool CheckForFullHouse(Hand hand)
        {
            IEnumerable<IGrouping<Rank, Card.Card>> groupedByRank = hand.Cards.GroupBy(x => x.Rank);

            bool hasThreeOfAKind = false;
            bool hasPair = false;

            foreach (IGrouping<Rank, Card.Card> group in groupedByRank)
            {
                if (group.Count() == 3)
                {
                    hasThreeOfAKind = true;
                }
                else if (group.Count() == 2)
                {
                    hasPair = true;
                }
            }

            return hasThreeOfAKind && hasPair;
        }

        private bool CheckForFlush(Hand hand)
        {
            Suit suit = hand.Cards[0].Suit;
            IEnumerable<Card.Card> cardsWithSameSuit = hand.Cards.Where(x => x.Suit == suit);

            return cardsWithSameSuit.Count() == 5;
        }

        private bool CheckForStraight(Hand hand)
        {
            IOrderedEnumerable<Card.Card> orderedCards = hand.Cards.OrderBy(x => x.Rank);

            int lastValue = (int)orderedCards.First().Rank - 1;

            foreach (Card.Card card in orderedCards)
            {
                int cardRank = (int)card.Rank;
                if (cardRank != lastValue + 1)
                {
                    return false;
                }
                lastValue = cardRank;
            }
            return true;
        }

        private bool CheckForThreeOfAKind(Hand hand)
        {
            IEnumerable<IGrouping<Rank, Card.Card>> groupedByRank = hand.Cards.GroupBy(x => x.Rank);

            foreach (IGrouping<Rank, Card.Card> group in groupedByRank)
            {
                if (group.Count() == 3)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckForTwoPairs(Hand hand)
        {
            IEnumerable<IGrouping<Rank, Card.Card>> groupedByRank = hand.Cards.GroupBy(x => x.Rank);
            int pairCount = 0;

            foreach (IGrouping<Rank, Card.Card> group in groupedByRank)
            {
                if (group.Count() == 2)
                {
                    pairCount += 1;
                }
            }

            return pairCount == 2;
        }

        private bool CheckForPair(Hand hand)
        {
            IEnumerable<IGrouping<Rank, Card.Card>> groupedByRank = hand.Cards.GroupBy(x => x.Rank);

            foreach (IGrouping<Rank, Card.Card> group in groupedByRank)
            {
                if (group.Count() == 2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
