using System;
using System.Collections.Generic;
using _src.CodeBase.Data;
using _src.CodeBase.Net;
using _src.CodeBase.UI;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _src.CodeBase.GameLogic {

    [RequireComponent(typeof(NetworkMatch))]
    public class Player : NetworkBehaviour {

        public static Player localPlayer;
        [SyncVar] public string matchID;
        [SyncVar] public string PlayerName;
        [SyncVar] public int playerIndex;

        NetworkMatch networkMatch;

        [SyncVar] public Match currentMatch;

        [SerializeField] GameObject playerLobbyUI;
        [SerializeField] GameObject canvas;
        [SerializeField] private Image _memImage;

        [SerializeField] 
        private TMP_InputField _inputField;

        [SerializeField] 
        private Button _sendButton;

        [SerializeField] 
        private Transform _variantsPanel;

        [SerializeField] 
        private VariantButton _variantButtonPrefab;

        [SerializeField] 
        private TextMeshProUGUI _playerScoreText;

        [SerializeField] 
        private ImagesData _imagesData;


        private List<VariantButton> _spawnedVariantButtons;
        

        public bool myTurn { get; private set; }

        void Awake () {
            networkMatch = GetComponent<NetworkMatch> ();
            _spawnedVariantButtons = new List<VariantButton>();
        }

        private void Start() => 
            _sendButton.onClick.AddListener(SendMessage);

        public override void OnStartClient () {
            if (isLocalPlayer) {
                localPlayer = this;
                Debug.Log ($"Its spawn my self");
            } else {
                Debug.Log ($"Spawning other player UI Prefab");
                playerLobbyUI = UILobby.instance.SpawnPlayerUIPrefab (this);
            }
        }

        public override void OnStopClient () {
            Debug.Log ($"Client with index {playerIndex} Stopped");
            ClientDisconnect ();
        }

        public override void OnStopServer () {
            Debug.Log ($"Client Stopped on Server");
            ServerDisconnect ();
        }

        /* 
            HOST MATCH
        */

        public void HostGame(bool publicMatch, string playerName) {
            string matchID = MatchMaker.GetRandomMatchID ();
            CmdHostGame (matchID, publicMatch, playerName);
        }

        [Command]
        void CmdHostGame(string _matchID, bool publicMatch, string playerName) {
            matchID = _matchID;
            if (MatchMaker.instance.HostGame (_matchID, this, publicMatch, out playerIndex, playerName)) {
                Debug.Log ($"<color=green>Game hosted successfully</color>");
                networkMatch.matchId = _matchID.ToGuid ();
                TargetHostGame (true, _matchID, playerIndex, playerName);
            } else {
                Debug.Log ($"<color=red>Game hosted failed</color>");
                TargetHostGame (false, _matchID, playerIndex, "");
            }
        }

        [TargetRpc]
        void TargetHostGame(bool success, string _matchID, int _playerIndex, string _playerName) {
            playerIndex = _playerIndex;
            matchID = _matchID;
            PlayerName = _playerName;
            Debug.Log ($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.HostSuccess (success, _matchID);
        }

        /* 
            JOIN MATCH
        */

        public void JoinGame(string matchId, string playerName) {
            CmdJoinGame (matchId, playerName);
        }

        [Command]
        void CmdJoinGame(string matchId, string playerName) {
            matchID = matchId;
            if (MatchMaker.instance.JoinGame (matchId, this, out playerIndex, playerName)) {
                Debug.Log ($"<color=green>Game Joined successfully</color>");
                networkMatch.matchId = matchId.ToGuid ();
                TargetJoinGame (true, matchId, playerIndex, playerName);
            } else {
                Debug.Log ($"<color=red>Game Joined failed</color>");
                TargetJoinGame (false, matchId, playerIndex, playerName);
            }
        }

        [TargetRpc]
        void TargetJoinGame(bool success, string _matchID, int _playerIndex, string _playerName) {
            playerIndex = _playerIndex;
            matchID = _matchID;
            PlayerName = _playerName;
            Debug.Log ($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.JoinSuccess (success, _matchID);
        }

        /* 
            DISCONNECT
        */

        public void DisconnectGame () {
            CmdDisconnectGame ();
        }

        [Command]
        void CmdDisconnectGame () {
            ServerDisconnect ();
        }

        void ServerDisconnect () {
            MatchMaker.instance.PlayerDisconnected (this, matchID);
            RpcDisconnectGame ();
            networkMatch.matchId = string.Empty.ToGuid ();
        }

        [ClientRpc]
        void RpcDisconnectGame () {
            ClientDisconnect ();
        }

        void ClientDisconnect () {
            if (playerLobbyUI != null) {
                Destroy (playerLobbyUI);
            }
        }

        /* 
            SEARCH MATCH
        */

        public void SearchGame () {
            CmdSearchGame ();
        }

        [Command]
        void CmdSearchGame () {
            if (MatchMaker.instance.SearchGame (this, out playerIndex, out matchID)) {
                Debug.Log ($"<color=green>Game Found Successfully</color>");
                networkMatch.matchId = matchID.ToGuid ();
                TargetSearchGame (true, matchID, playerIndex);
            } else {
                Debug.Log ($"<color=red>Game Search Failed</color>");
                TargetSearchGame (false, matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetSearchGame (bool success, string _matchID, int _playerIndex) {
            playerIndex = _playerIndex;
            matchID = _matchID;
            Debug.Log ($"MatchID: {matchID} == {_matchID} | {success}");
            UILobby.instance.SearchGameSuccess (success, _matchID);
        }

        /* 
            BEGIN MATCH
        */

        public void BeginGame () {
            CmdBeginGame ();
        }

        [Command]
        void CmdBeginGame () {
            MatchMaker.instance.BeginGame (matchID);
            Debug.Log ($"<color=red>Game Beginning</color>");
        }

        public void StartGame () { //Server
            TargetBeginGame ();
        }

        [TargetRpc]
        void TargetBeginGame () {
            Debug.Log ($"MatchID: {matchID} | Beginning");
            //Additively load game scene
            SceneManager.LoadScene (2, LoadSceneMode.Additive);
            canvas.SetActive (true);
        }

        /*
            TURN
        */

        public void SetTurn (bool _myTurn) {
            myTurn = _myTurn; //Set myTurn on server
            RpcSetTurn (_myTurn);
        }

        [ClientRpc]
        void RpcSetTurn (bool _myTurn) {
            Debug.Log ($"{(isLocalPlayer ? "localPlayer" : "other player")}'s turn.");

            myTurn = _myTurn; //Set myTurn on clients
        }

        /*
            GAMEPLAY
        */

        public void DoSomething () {
            if (myTurn) { //Check that it is your turn
                CmdDoSomething ();
            } else {
                Debug.Log ($"It is not your turn!");
            }
        }

        private void SendMessage()
        {
            string message = _inputField.text;
            if (message == String.Empty)
                return;
            
            _sendButton.gameObject.SetActive(false);
            
            CmdSendMessage(message);
        }

        [Command]
        void CmdDoSomething () {
            RpcDoSomething ();
        }

        [ClientRpc]
        void RpcDoSomething () {
            Debug.Log ($"{(isLocalPlayer ? "localPlayer" : "other player")} is doing something.");
        }

        [TargetRpc]
        public void SetImage(int imageIndex)
        {
            _memImage.gameObject.SetActive(true);
            _memImage.sprite = _imagesData.SpritesImages[imageIndex];
        }

        [Command]
        private void CmdSendMessage(string message)
        {
            TurnManager.instance.OnMessageSent(this, message);
        }

        [TargetRpc]
        public void AcceptAnswer()
        {
            _inputField.text = "";
            _inputField.gameObject.SetActive(false);
        }

        [TargetRpc]
        public void ActivateAnswerTools()
        {
            _inputField.gameObject.SetActive(true);
            _sendButton.gameObject.SetActive(true);
        }

        [TargetRpc]
        public void DeactivateAnswering()
        {
            _memImage.gameObject.SetActive(false);
        }

        [TargetRpc]
        public void AddUserVariant(string messageVariant)
        {
            VariantButton variantButtonInstance = Instantiate(_variantButtonPrefab, _variantsPanel);
            variantButtonInstance.SetText(messageVariant);
            variantButtonInstance.Button.onClick.AddListener(OnClickVariantButton);

            _spawnedVariantButtons.Add(variantButtonInstance);
        }

        private void OnClickVariantButton()
        {
            string selectedVariantText = "";
            foreach (VariantButton variantButton in _spawnedVariantButtons)
            {
                if (variantButton.IsSelected)
                    selectedVariantText = variantButton.VariantText;

                variantButton.SetUnselectable();
            } 

            CmdSendSelectedVariant(selectedVariantText);
        }

        [Command]
        private void CmdSendSelectedVariant(string variantText)
        {
            TurnManager.instance.SelectVariant(variantText);
        }

        [TargetRpc]
        public void ActivatePlayerScore()
        {
            _playerScoreText.gameObject.SetActive(true);
            _playerScoreText.text = $"Score: {0}";
        }

        [TargetRpc]
        public void UpdateScore(int score)
        {
            RemoveAllVariableButtons();

            _playerScoreText.text = $"Score: {score.ToString()}";
        }

        private void RemoveAllVariableButtons()
        {
            int countOfVariableButtons = _spawnedVariantButtons.Count;
            for (int i = 0; i < countOfVariableButtons; i++)
            {
                Destroy(_spawnedVariantButtons[0].gameObject);
                _spawnedVariantButtons.RemoveAt(0);
            }
        }
    }
}