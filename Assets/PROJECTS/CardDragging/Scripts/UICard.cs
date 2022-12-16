using System.Collections;
using System.Collections.Generic;
using MirrorBasics;
using UnityEngine;
using UnityEngine.UI;

namespace CardDragging {
    public class UICard : MonoBehaviour {

        [SerializeField] Card card;
        [SerializeField] Image cardImage;
        [SerializeField] CardSprites cardSprites;
        [SerializeField] Canvas canvas;

        UIPlayerHand uIPlayerHand;

        int dragSortOrderOrig = 0;

        public void SetUIPlayerHand (UIPlayerHand _uIPlayerHand) {
            uIPlayerHand = _uIPlayerHand;
        }

        public void SetCard (Card card /* , int index */ ) {
            this.card = card;

            for (int i = 0; i < cardSprites.cardSprites.Count; i++) {
                if (cardSprites.cardSprites[i].card.value == card.value && cardSprites.cardSprites[i].card.suit == card.suit) {
                    cardImage.sprite = cardSprites.cardSprites[i].sprite;
                    if (card.suit == CardSuit.Diamonds || card.suit == CardSuit.Hearts) {
                        cardImage.color = new Color (255f / 255f, 55f / 255f, 55f / 255f);
                    }
                    break;
                }
            }

            // Debug.Log ($"Index: {index} | {(3 - (index-1))}");
            // ((RectTransform) transform).anchoredPosition += new Vector2 (0f, 30f) * index;
            // canvas.sortingOrder = (3 - (index - 1));
        }

        public void Drag () {
            Debug.Log ($"Drag");
            transform.parent = uIPlayerHand.draggingParent;
            ((RectTransform) transform).anchorMin = Vector2.zero;
            ((RectTransform) transform).anchorMax = Vector2.zero;
            dragSortOrderOrig = canvas.sortingOrder;
            canvas.sortingOrder = 99;
        }

        public void Dragging () {
            Debug.Log ($"Dragging");
            ((RectTransform) transform).anchoredPosition = Input.mousePosition;
        }

        public void Drop () {
            Debug.Log ($"Drop");
            canvas.sortingOrder = dragSortOrderOrig;

            float dropY = Input.mousePosition.y;

            if (dropY >= 200) {
                Debug.Log ($"Dropping on table");

                Player.localPlayer.PlaceCardOnTable (card);
                Destroy (gameObject);

            } else {
                Debug.Log ($"Dropping back in hand");

                for (int i = 0; i < uIPlayerHand.handTransform.childCount; i++) {
                    float adjustedX = (((RectTransform) transform).anchoredPosition.x + (((RectTransform) transform).sizeDelta.x / 2));
                    float prevPosX = uIPlayerHand.handTransform.GetChild (i).position.x;
                    float nextPosX = uIPlayerHand.handTransform.GetChild (i + 1).position.x + (((RectTransform) transform).sizeDelta.x / 2);
                    Debug.Log ($"Index: {i} | Adjusted X: {adjustedX} | prevPosX {prevPosX} | nextPosX {nextPosX}");
                    if (adjustedX >= prevPosX && adjustedX < nextPosX) {
                        transform.parent = uIPlayerHand.handTransform;
                        transform.SetSiblingIndex (i + 1);
                        break;
                    }
                }
            }

            //Networking magic
        }

    }
}