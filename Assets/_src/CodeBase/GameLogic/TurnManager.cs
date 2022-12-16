using System.Collections;
using System.Collections.Generic;
using _src.CodeBase.Data;
using UnityEngine;

namespace _src.CodeBase.GameLogic {
    public class TurnManager : MonoBehaviour {

        [SerializeField] List<Player> players;
        [SerializeField] Player currentPlayer;
        [SerializeField] int currentPlayerIndex;
        [SerializeField] bool gameInProgress = true;
        [SerializeField] private ImagesData _imagesData;

        public void ManagePlayers (List<Player> _players) {
            players = _players;
            // StartCoroutine (TrackTurns ());
            StartCoroutine(ChangeImagesRoutine());
        }

        private IEnumerator ChangeImagesRoutine()
        {
            while (gameInProgress)
            {
                int randomImageIndex = Random.Range(0, _imagesData.SpritesImages.Count);
                
                foreach (Player player in players)
                {
                    player.SetImage(randomImageIndex);
                }

                yield return new WaitForSeconds (1);
            }
        }

        IEnumerator TrackTurns () {
            currentPlayerIndex = Random.Range (0, players.Count);
            currentPlayer = players[currentPlayerIndex];

            while (gameInProgress) {
                currentPlayer.SetTurn (true);
                for (var i = 0; i < players.Count; i++) {
                    if (players[i] != currentPlayer) players[i].SetTurn (false);
                }

                yield return new WaitForSeconds (10);
                NextPlayer ();
            }
        }

        void NextPlayer () {
            currentPlayerIndex++;
            if (currentPlayerIndex >= players.Count) {
                currentPlayerIndex = 0;
            }

            currentPlayer = players[currentPlayerIndex];
        }

    }
}