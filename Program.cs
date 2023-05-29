using System;
using System.Collections.Generic;

public class Card
{
    public string Suit { get; }
    public string Rank { get; }

    public Card(string suit, string rank)
    {
        Suit = suit;
        Rank = rank;
    }
}

public class Deck<T>
{
    private List<T> cards;

    public int Count => cards.Count;

    public Deck(List<T> initialCards)
    {
        cards = initialCards;
    }

    public T DealCard()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("The deck is empty.");
        }

        T card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    public void Shuffle()
    {
        Random random = new Random();
        int n = Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }
}

public class Game
{
    private List<Player> players;
    private Deck<Card> deck;
    private List<Card> pile;

    public Game(List<string> playerNames)
    {
        players = new List<Player>();
        foreach (string name in playerNames)
        {
            players.Add(new Player(name));
        }

        List<Card> initialCards = GenerateDeck();
        deck = new Deck<Card>(initialCards);
        deck.Shuffle();
        pile = new List<Card>();
    }

    public void Play()
    {
        while (true)
        {
            foreach (Player player in players)
            {
                if (deck.Count == 0)
                {
                    Console.WriteLine("The deck is empty.");
                    return;
                }

                Card card = deck.DealCard();
                player.AddCardToHand(card);
                Console.WriteLine($"{player.Name} dealt {card.Rank} of {card.Suit}.");

                if (pile.Count > 0)
                {
                    if (IsWar(card))
                    {
                        Console.WriteLine("War!");

                        for (int i = 0; i < 2; i++)
                        {
                            if (deck.Count == 0)
                            {
                                Console.WriteLine("The deck is empty.");
                                return;
                            }

                            Card warCard = deck.DealCard();
                            player.AddCardToHand(warCard);
                            Console.WriteLine($"{player.Name} dealt {warCard.Rank} of {warCard.Suit}.");
                        }

                        pile.Add(card);
                        CompareWarCards();
                    }
                    else
                    {
                        pile.Add(card);
                        Player winner = FindRoundWinner();
                        if (winner != null)
                        {
                            Console.WriteLine($"{winner.Name} won the round.");
                            winner.AddCardsToHand(pile);
                            pile.Clear();
                        }
                    }
                }
                else
                {
                    pile.Add(card);
                }
            }
        }
    }

    private List<Card> GenerateDeck()
    {
        List<Card> cards = new List<Card>();
        string[] suits = { "Spades", "Hearts", "Diamonds", "Clubs" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                cards.Add(new Card(suit, rank));
            }
        }

        return cards;
    }

    private bool IsWar(Card card)
    {
        foreach (Card pileCard in pile)
        {
            if (pileCard.Rank != card.Rank)
            {
                return false;
            }
        }

        return true;
    }

    private void CompareWarCards()
    {
        List<Player> warPlayers = new List<Player>();

        foreach (Player player in players)
        {
            if (player.HasCards())
            {
                warPlayers.Add(player);
            }
        }

        if (warPlayers.Count < 2)
        {
            Console.WriteLine("There are not enough players to continue the war.");
            return;
        }

        List<Card> warCards = new List<Card>();

        foreach (Player player in warPlayers)
        {
            Card warCard = player.PlayCard();
            if (warCard != null)
            {
                warCards.Add(warCard);
                Console.WriteLine($"{player.Name} played {warCard.Rank} of {warCard.Suit}.");
            }
        }

        if (warCards.Count > 0)
        {
            Card highestCard = GetHighestCard(warCards);
            Player winner = GetPlayerWithCard(warPlayers, highestCard);
            if (winner != null)
            {
                Console.WriteLine($"{winner.Name} won the war.");
                winner.AddCardsToHand(pile);
                winner.AddCardsToHand(warCards);
                pile.Clear();
            }
        }
    }

    private Player FindRoundWinner()
    {
        List<Player> activePlayers = new List<Player>();

        foreach (Player player in players)
        {
            if (player.HasCards())
            {
                activePlayers.Add(player);
            }
        }

        if (activePlayers.Count < 2)
        {
            Console.WriteLine("The game is over.");
            return null;
        }

        List<Card> roundCards = new List<Card>();

        foreach (Player player in activePlayers)
        {
            Card roundCard = player.PlayCard();
            if (roundCard != null)
            {
                roundCards.Add(roundCard);
                Console.WriteLine($"{player.Name} played {roundCard.Rank} of {roundCard.Suit}.");
            }
        }

        if (roundCards.Count > 0)
        {
            Card highestCard = GetHighestCard(roundCards);
            Player winner = GetPlayerWithCard(activePlayers, highestCard);
            return winner;
        }

        return null;
    }

    private Card GetHighestCard(List<Card> cards)
    {
        Card highestCard = cards[0];

        foreach (Card card in cards)
        {
            if (CompareCards(card, highestCard) > 0)
            {
                highestCard = card;
            }
        }

        return highestCard;
    }

    private Player GetPlayerWithCard(List<Player> players, Card card)
    {
        foreach (Player player in players)
        {
            if (player.HasCard(card))
            {
                return player;
            }
        }

        return null;
    }

    private int CompareCards(Card card1, Card card2)
    {
        string[] ranks = { "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        int index1 = Array.IndexOf(ranks, card1.Rank);
        int index2 = Array.IndexOf(ranks, card2.Rank);

        return index1 - index2;
    }
}

public class Player
{
    public string Name { get; }
    private List<Card> hand;

    public Player(string name)
    {
        Name = name;
        hand = new List<Card>();
    }

    public void AddCardToHand(Card card)
    {
        hand.Add(card);
    }

    public void AddCardsToHand(List<Card> cards)
    {
        hand.AddRange(cards);
    }

    public Card PlayCard()
    {
        if (hand.Count == 0)
        {
            return null;
        }

        Card card = hand[0];
        hand.RemoveAt(0);
        return card;
    }

    public bool HasCards()
    {
        return hand.Count > 0;
    }

    public bool HasCard(Card card)
    {
        return hand.Contains(card);
    }
}

public class Program
{
    public static void Main()
    {
        List<string> playerNames = new List<string> { "Player 1", "Player 2", "Player 3" };
        Game game = new Game(playerNames);
        game.Play();
    }
}
