using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TurnManager : NetworkBehaviour {

    public static TurnManager instance;

    [SerializeField] List<Player> players = new List<Player> ();
    public Player currentPlayer;

    [SerializeField] Deck deck = new Deck ();

    bool allPlayersAdded = false;

    void Awake () {
        instance = this;
    }

    void Start () {
        deck = new Deck ();
        StartCoroutine (WaitForAllPlayers ());
    }

    public void AddPlayer (Player player) {
        players.Add (player);
        Debug.Log ($"Player Added: {players.Count}");

        if (players.Count == NetworkServer.connections.Count) {
            allPlayersAdded = true;
        }
    }

    IEnumerator WaitForAllPlayers () {
        while (!allPlayersAdded) {
            yield return null;
        }

        int cardsPerPlayer = 0;

        if (players.Count <= 3) {
            cardsPerPlayer = 7;
        } else {
            cardsPerPlayer = 5;
        }

        for (int i = 0; i < cardsPerPlayer; i++) {
            foreach (var player in players) {
                player.DealCard (deck.GetCard ());
            }
        }

        currentPlayer = players[Random.Range (0, players.Count)];
    }

}