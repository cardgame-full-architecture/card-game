using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICard : MonoBehaviour {

    [SerializeField] Card card;
    [SerializeField] Image cardImage;
    [SerializeField] CardSprites cardSprites;
    [SerializeField] Canvas canvas;

    public void SetCard (Card card, int index) {
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

        Debug.Log ($"Index: {index} | {(3 - (index-1))}");
        ((RectTransform) transform).anchoredPosition += new Vector2 (0f, 30f) * index;
        canvas.sortingOrder = (3 - (index - 1));

    }

}