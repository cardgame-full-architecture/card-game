using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Mirror;

[System.Serializable]
public class Card {

    public CardSuit suit;
    public CardValue value;

    public Card (CardSuit suit, CardValue value) {
        this.suit = suit;
        this.value = value;
    }

    public Card () { }

}

[System.Serializable]
public class SyncListCard : SyncList<Card> { }

[System.Serializable]
public class Deck {

    public SyncListCard cards = new SyncListCard ();

    public Deck () {
        cards = new SyncListCard ();

        for (int suit = 0; suit < Enum.GetValues (typeof (CardSuit)).Length; suit++) {
            for (int value = 0; value < Enum.GetValues (typeof (CardValue)).Length; value++) {
                cards.Add (new Card ((CardSuit) suit, (CardValue) value));
            }
        }

        Shuffle ();
    }

    public Card GetCard () {
        Card card = cards[0];
        cards.RemoveAt (0);
        return card;
    }

    public void Shuffle () {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider ();
        int n = cards.Count;
        while (n > 1) {
            byte[] box = new byte[1];
            do provider.GetBytes (box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }

}

[System.Serializable]
public class Book {
    public CardValue bookValue;
    public SyncListCard cards = new SyncListCard ();

    public Book (Card card) {
        this.bookValue = card.value;
        cards.Add (card);
    }

    public Book () { }
}

[System.Serializable]
public class SyncListBook : SyncList<Book> { }

[System.Serializable]
public enum CardSuit : Byte {
    Spades,
    Clubs,
    Diamonds,
    Hearts
}

[System.Serializable]
public enum CardValue : Byte {
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
    Ace,
}