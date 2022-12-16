using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardDragging {
    public class UIPlayerHand : MonoBehaviour {

        [SerializeField] List<Transform> seating = new List<Transform> ();
        [SerializeField] GameObject cardBackPrefab;
        Transform selectedSeat;
        public Transform draggingParent;
        public Transform handTransform;
        public Transform tableHandTransform;

        public void SitDown (int playerIndex, int playerCount) { //index: 0 | players: 2
            int targetIndex = playerCount - (playerIndex + 1);

            if (targetIndex > playerCount - 1) {
                targetIndex = 0;
            }
            if (targetIndex < 0) {
                targetIndex = (playerCount - 1) + targetIndex;
            }

            Debug.Log ($"playerCount {playerCount} | targetIndex {targetIndex}");

            selectedSeat = seating[targetIndex];

            for (int i = 0; i < playerCount - 1; i++) {
                seating[i].gameObject.SetActive (true);
            }
        }

        public void SpawnCardBack () {
            GameObject newCard = Instantiate (cardBackPrefab, selectedSeat);

        }

    }
}