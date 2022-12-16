using _src.CodeBase.Data;
using _src.CodeBase.Net;
using _src.CodeBase.UI;
using Mirror;
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
        private ImagesData _imagesData;

        public bool myTurn { get; private set; }

        void Awake () {
            networkMatch = GetComponent<NetworkMatch> ();
        }

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
            _memImage.sprite = _imagesData.SpritesImages[imageIndex];
        }
    }
}