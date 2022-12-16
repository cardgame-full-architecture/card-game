using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace CardDragging {
    [RequireComponent(typeof(NetworkMatch))]
    public class TurnManager : NetworkBehaviour {

        public static TurnManager instance;

        public Deck deck = new Deck ();

        List<Player> players = new List<Player> ();

        public SyncListCard tableHand = new SyncListCard ();

        void Start () {
            if (!isServer) return;

            instance = this;

            deck.Shuffle ();

            Debug.Log ($"Dealing cards {players.Count}");

            int currentPlayerIndex = 0;
            while (deck.cards.Count > 0) {
                Card card = deck.cards[0];
                deck.cards.RemoveAt (0);
                players[currentPlayerIndex].ReceiveCard (card);

                currentPlayerIndex++;
                if (currentPlayerIndex > players.Count - 1) {
                    currentPlayerIndex = 0;
                }
            }
        }

        //Matchmaker
        public void AddPlayer (Player _player) {
            Debug.Log ($"Adding player");
            players.Add (_player);
        }

        public void PlaceCardOnTable (Card card) {
            tableHand.Add (card);
            foreach (var player in players) {
                player.PlacedCardOnTable (card);
            }
        }

    }
}