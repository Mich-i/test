namespace PokerHandKata.Card;

public class Card
{
    public Card(Suit suit, Rank rank)
    {
        this.Suit = suit;
        this.Rank = rank;
    }

    public Suit Suit { get; }
    public Rank Rank { get; }
}