using UnityEngine;
using UnityEngine.UI;

namespace _src.CodeBase.UI {

    public class UIPlayer : MonoBehaviour {

        [SerializeField] Text text;
        GameLogic.Player player;

        public void SetPlayer (GameLogic.Player player) {
            this.player = player;
            text.text = "Player " + player.playerIndex.ToString ();
        }

    }
}