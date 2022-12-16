using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardSprite {
    public Card card;
    public Sprite sprite;

    public CardSprite (Card card) {
        this.card = card;
        this.sprite = null;
    }
}

[CreateAssetMenu (fileName = "CardSprites", menuName = "Data/CardSprites", order = 0)]
public class CardSprites : ScriptableObject {
    public List<CardSprite> cardSprites = new List<CardSprite> ();

    [ContextMenu("Reset Deck")]
    public void ResetDeck () {
        cardSprites = new List<CardSprite> ();

        for (int suit = 0; suit < Enum.GetValues (typeof (CardSuit)).Length; suit++) {
            for (int value = 0; value < Enum.GetValues (typeof (CardValue)).Length; value++) {
                cardSprites.Add (new CardSprite (new Card ((CardSuit) suit, (CardValue) value)));
            }
        }
    }
}