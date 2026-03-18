using AwesomeAssertions;
using PokerHandKata;
using PokerHandKata.Card;

namespace PokerHandKataTests
{
    public class PokerHandTests
    {
        [Theory]
        [InlineData(Combination.HighCard)]
        [InlineData(Combination.Pair)]
        [InlineData(Combination.TwoPairs)]
        [InlineData(Combination.ThreeOfAKind)]
        [InlineData(Combination.Straight)]
        [InlineData(Combination.Flush)]
        [InlineData(Combination.FullHouse)]
        [InlineData(Combination.FourOfAKind)]
        [InlineData(Combination.StraightFlush)]
        public void HighCardShouldBeCalculated(Combination combination)
        {
            // Arrange
            Hand hand = this.CreateHand(combination);
            HandCalculator testee = new();

            // Act
            Combination calculateHand = testee.CalculateHand(hand);

            // Assert
            calculateHand.Should().Be(combination);
        }

        private Hand CreateHand(Combination combination)
        {
            switch (combination)
            {
                case Combination.HighCard:
                    Card[] highCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.D, Rank.Eight),
                        new(Suit.C, Rank.Seven),
                        new(Suit.S, Rank.Three),
                        new(Suit.H, Rank.Two)
                    ];

                    return new Hand(highCards);

                case Combination.Pair:
                    Card[] pairCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.D, Rank.A),
                        new(Suit.C, Rank.Seven),
                        new(Suit.S, Rank.Three),
                        new(Suit.H, Rank.Two)
                    ];

                    return new Hand(pairCards);    

                case Combination.TwoPairs:
                    Card[] twoPairsCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.D, Rank.A),
                        new(Suit.C, Rank.K),
                        new(Suit.S, Rank.K),
                        new(Suit.H, Rank.Two)
                    ];

                    return new Hand(twoPairsCards);

                case Combination.ThreeOfAKind:
                    Card[] threeOfAKindCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.D, Rank.A),
                        new(Suit.C, Rank.A),
                        new(Suit.S, Rank.K),
                        new(Suit.H, Rank.Q)
                    ];

                    return new Hand(threeOfAKindCards);

                case Combination.Straight:
                    Card[] straightCards =
                    [
                        new(Suit.H, Rank.Six),
                        new(Suit.D, Rank.Five),
                        new(Suit.C, Rank.Four),
                        new(Suit.S, Rank.Three),
                        new(Suit.H, Rank.Two)
                    ];

                    return new Hand(straightCards);

                case Combination.Flush:
                    Card[] flushCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.H, Rank.K),
                        new(Suit.H, Rank.Q),
                        new(Suit.H, Rank.J),
                        new(Suit.H, Rank.Nine)
                    ];

                    return new Hand(flushCards);

                case Combination.FullHouse:
                    Card[] fullHouseCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.D, Rank.A),
                        new(Suit.C, Rank.A),
                        new(Suit.S, Rank.K),
                        new(Suit.H, Rank.K)
                    ];

                    return new Hand(fullHouseCards);

                case Combination.FourOfAKind:
                    Card[] fourOfAKindCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.D, Rank.A),
                        new(Suit.C, Rank.A),
                        new(Suit.S, Rank.A),
                        new(Suit.H, Rank.K)
                    ];

                    return new Hand(fourOfAKindCards);

                case Combination.StraightFlush:
                    Card[] straightFlushCards =
                    [
                        new(Suit.H, Rank.Six),
                        new(Suit.H, Rank.Five),
                        new(Suit.H, Rank.Four),
                        new(Suit.H, Rank.Three),
                        new(Suit.H, Rank.Two)
                    ];

                    return new Hand(straightFlushCards);

                default:
                    Card[] defaultCards =
                    [
                        new(Suit.H, Rank.A),
                        new(Suit.D, Rank.Eight),
                        new(Suit.C, Rank.Seven),
                        new(Suit.S, Rank.Three),
                        new(Suit.H, Rank.Two)
                    ];
                    return new Hand(defaultCards);
            }
        }
    }
}
