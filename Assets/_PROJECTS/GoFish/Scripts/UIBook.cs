using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBook : MonoBehaviour {

    public GameObject uiCardPrefab;

    int cardCount = 0;

    public Book book;
    [SerializeField] List<UICard> uiCards = new List<UICard> ();

    void Update () {
        if (cardCount != book.cards.Count) {
            UpdateUICards ();
        }
    }

    public void SetBook (Book book) {
        Debug.Log($"Book cards: {book.cards.Count}");
        this.book = book;
    }

    void UpdateUICards () {
        int cardsToAdd = book.cards.Count - uiCards.Count;
        for (int i = cardsToAdd; i > 0; i--) {
            GameObject newCard = Instantiate (uiCardPrefab, transform);
            newCard.GetComponent<UICard> ().SetCard (book.cards[book.cards.Count - i], book.cards.Count - i);
        }
        cardCount = book.cards.Count;
    }

}