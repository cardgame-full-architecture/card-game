using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _src.CodeBase.UI {

    public class UILobby : MonoBehaviour {

        public static UILobby instance;

        [Header("Host Join")] 
        [SerializeField] private InputField playerNameInput;
        [SerializeField] InputField joinMatchInput;
        [SerializeField] List<Selectable> lobbySelectables = new List<Selectable> ();
        [SerializeField] Canvas lobbyCanvas;
        [SerializeField] Canvas searchCanvas;
        bool searching = false;

        [Header ("Lobby")]
        [SerializeField] Transform UIPlayerParent;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;
        [SerializeField] GameObject beginGameButton;

        GameObject localPlayerLobbyUI;

        void Start () {
            instance = this;
            
            if (Application.isBatchMode)
                return;
            
            StartCoroutine(StartLobbyRoutine());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                DisconnectGame();
        }

        private IEnumerator StartLobbyRoutine()
        {
            yield return new WaitForSeconds(1);

            if (PlayerPrefs.GetInt("IsCrashed") == 1)
                JoinGameWithHost(PlayerPrefs.GetString("Name"), PlayerPrefs.GetString("RoomId"));
            else if (PlayerPrefs.GetInt("IsHost") == 1)
                HostPublic(PlayerPrefs.GetString("Name"));
            else
                Join(PlayerPrefs.GetString("Name"), PlayerPrefs.GetString("RoomId"));
        }

        private void JoinGameWithHost(string playerName, string roomId)
        {
            PlayerPrefs.SetInt("IsCrashed", 0);
            
            lobbySelectables.ForEach (x => x.interactable = false);

            GameLogic.Player.localPlayer.JoinGameWithHost (roomId, playerName);
        }

        public void HostPublic ()
        {
            string playerName = playerNameInput.text;
            if (playerName == String.Empty)
                return;
            
            lobbySelectables.ForEach (x => x.interactable = false);

            GameLogic.Player.localPlayer.HostGame (true, playerName);
        }
        
        public void HostPublic (string playerName)
        {
            if (playerName.Trim() == String.Empty)
                return;

            lobbySelectables.ForEach (x => x.interactable = false);

            GameLogic.Player.localPlayer.HostGame (true, playerName);
        }

        public void HostPrivate ()
        {
            string playerName = playerNameInput.text;
            if (playerName == String.Empty)
                return;

            lobbySelectables.ForEach (x => x.interactable = false);

            GameLogic.Player.localPlayer.HostGame (false, playerName);
        }

        public void HostSuccess (bool success, string matchID) {
            if (success) {
                lobbyCanvas.enabled = true;

                if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
                localPlayerLobbyUI = SpawnPlayerUIPrefab (GameLogic.Player.localPlayer);
                matchIDText.text = matchID;
                beginGameButton.SetActive (true);

                PlayerPrefs.SetString("RoomId", matchID);
            } else {
                lobbySelectables.ForEach (x => x.interactable = true);
            }
        }

        public void Join () {
            string playerName = playerNameInput.text;
            if (playerName == String.Empty)
                return;
            
            lobbySelectables.ForEach (x => x.interactable = false);

            GameLogic.Player.localPlayer.JoinGame (joinMatchInput.text.ToUpper (), playerName);
        }
        
        public void Join (string playerName, string roomId) {
            lobbySelectables.ForEach (x => x.interactable = false);

            GameLogic.Player.localPlayer.JoinGame (roomId.ToUpper(), playerName);
        }

        public void JoinSuccess (bool success, string matchID) {
            if (success) {
                lobbyCanvas.enabled = true;

                if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
                localPlayerLobbyUI = SpawnPlayerUIPrefab (GameLogic.Player.localPlayer);
                matchIDText.text = matchID;
            } else {
                lobbySelectables.ForEach (x => x.interactable = true);
            }
        }

        public void DisconnectGame ()
        {
            PlayerPrefs.SetString("Name", "");
            PlayerPrefs.SetString("RoomId", "");
            PlayerPrefs.SetInt("IsCrashed", 0);
            GameLogic.Player.localPlayer.StopClient();
            return;
            
            if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
            GameLogic.Player.localPlayer.DisconnectGame ();

            lobbyCanvas.enabled = false;
            lobbySelectables.ForEach (x => x.interactable = true);
            beginGameButton.SetActive (false);
        }

        public GameObject SpawnPlayerUIPrefab (GameLogic.Player player) {
            GameObject newUIPlayer = Instantiate (UIPlayerPrefab, UIPlayerParent);
            newUIPlayer.GetComponent<UIPlayer> ().SetPlayer (player);
            newUIPlayer.transform.SetSiblingIndex (player.playerIndex - 1);

            return newUIPlayer;
        }

        public void BeginGame () {
            GameLogic.Player.localPlayer.BeginGame ();
        }

        public void SearchGame () {
            StartCoroutine (Searching ());
        }

        public void CancelSearchGame () {
            searching = false;
        }

        public void SearchGameSuccess (bool success, string matchID) {
            if (success) {
                searchCanvas.enabled = false;
                searching = false;
                JoinSuccess (success, matchID);
            }
        }

        IEnumerator Searching () {
            searchCanvas.enabled = true;
            searching = true;

            float searchInterval = 1;
            float currentTime = 1;

            while (searching) {
                if (currentTime > 0) {
                    currentTime -= Time.deltaTime;
                } else {
                    currentTime = searchInterval;
                    GameLogic.Player.localPlayer.SearchGame ();
                }
                yield return null;
            }
            searchCanvas.enabled = false;
        }

    }
}