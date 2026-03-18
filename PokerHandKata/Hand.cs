namespace PokerHandKata
{
    public class Hand
    {
        public Hand(Card.Card[] cards)
        {
            if (cards.Length != 5)
            {
                throw new ArgumentException("A hand must consist of exactly 5 cards.");
            }

            this.Cards = cards;
        }
        public Card.Card[] Cards { get; }
    }
}
