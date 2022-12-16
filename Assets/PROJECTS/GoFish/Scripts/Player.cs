using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour {

    public List<Book> playerHand = new List<Book> ();
    [SerializeField] GameObject uiBookPrefab;
    [SerializeField] Transform uiBookParent;

    [SerializeField] List<UIBook> uiBooks = new List<UIBook> ();

    void Start () {
        if (isServer) {
            Debug.Log ($"Player adding to TurnManager");
            TurnManager.instance.AddPlayer (this);
        }
        if (isClient) {
            Debug.Log ($"Subscribing to hand update");
            StartCoroutine (CheckCardsAfter5Sec ());
        }
    }

    // Debug purposes
    IEnumerator CheckCardsAfter5Sec () {
        yield return new WaitForSeconds (5);
        Debug.Log ($"Player Hand: {playerHand.Count}");
        for (int i = 0; i < playerHand.Count; i++) {
            Debug.Log ($"Book value: {playerHand[i].bookValue}");
            for (int k = 0; k < playerHand[i].cards.Count; k++) {
                Debug.Log ($"{playerHand[i].cards[k].suit} {playerHand[i].cards[k].value}");
            }
        }
    }

    public void DealCard (Card card) { //Server
        bool match = false;
        int matchIndex = 0;

        for (int k = 0; k < playerHand.Count; k++) {
            if (card.value == playerHand[k].bookValue) {
                match = true;
                matchIndex = k;
                break;
            }
        }

        if (match) {
            playerHand[matchIndex].cards.Add (card);
        } else {
            Book newBook = new Book (card);
            playerHand.Add (newBook);
            GameObject newUIBook = Instantiate (uiBookPrefab, uiBookParent);
            UIBook book = newUIBook.GetComponent<UIBook> ();
            book.SetBook (newBook);
            uiBooks.Add (book);
        }

        RpcDealCardToClient (card);
    }

    [ClientRpc]
    void RpcDealCardToClient (Card card) {
        bool match = false;
        int matchIndex = 0;

        for (int k = 0; k < playerHand.Count; k++) {
            if (card.value == playerHand[k].bookValue) {
                match = true;
                matchIndex = k;
                break;
            }
        }

        if (match) {
            playerHand[matchIndex].cards.Add (card);
        } else {
            Book newBook = new Book (card);
            playerHand.Add (newBook);

            if (isLocalPlayer) {
                GameObject newUIBook = Instantiate (uiBookPrefab, uiBookParent);
                UIBook book = newUIBook.GetComponent<UIBook> ();
                book.SetBook (newBook);
                uiBooks.Add (book);
            }
        }
    }

    public Book GetCard (CardValue bait) {
        Book searchingBook = null;
        for (int k = 0; k < playerHand.Count; k++) {
            if (bait == playerHand[k].bookValue) {
                searchingBook = playerHand[k];
                playerHand.RemoveAt (k);
                break;
            }
        }
        return searchingBook;
    }

}