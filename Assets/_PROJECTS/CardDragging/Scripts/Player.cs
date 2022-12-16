using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardDragging {

    [RequireComponent(typeof(NetworkMatch))]
    public class Player : NetworkBehaviour {

        public static Player localPlayer;
        [SyncVar] public string matchID;
        [SyncVar] public int playerIndex;

        [SerializeField] SyncListCard hand = new SyncListCard ();

        [SerializeField] GameObject cardPrefab;

        NetworkMatch networkMatch;
        [SerializeField] UIPlayerHand uIPlayerHand;

        [SyncVar] public Match currentMatch;

        void Start () {
            networkMatch = GetComponent<NetworkMatch> ();

            if (isLocalPlayer) {
                localPlayer = this;
            } else {
                UILobby.instance.SpawnPlayerUIPrefab (this);
            }
        }

        /* 
            HOST MATCH
        */

        public void HostGame (bool publicMatch) {
            string matchID = MatchMaker.GetRandomMatchID ();
            CmdHostGame (matchID, publicMatch);
        }

        [Command]
        void CmdHostGame (string _matchID, bool publicMatch) {
            matchID = _matchID;
            if (MatchMaker.instance.HostGame (_matchID, this, publicMatch, out playerIndex)) {
                Debug.Log ($"<color = green>Game hosted successfully</color>");
                networkMatch.matchId = _matchID.ToGuid ();
                TargetHostGame (true, _matchID, playerIndex);
            } else {
                Debug.Log ($"<color = red>Game hosted failed</color>");
                TargetHostGame (false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetHostGame (bool success, string _matchID, int _playerIndex) {
            playerIndex = _playerIndex;
            matchID = _matchID;
            Debug.Log ($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.HostSuccess (success, _matchID);
        }

        /* 
            JOIN MATCH
        */

        public void JoinGame (string _inputID) {
            CmdJoinGame (_inputID);
        }

        [Command]
        void CmdJoinGame (string _matchID) {
            matchID = _matchID;
            if (MatchMaker.instance.JoinGame (_matchID, this, out playerIndex)) {
                Debug.Log ($"<color = green>Game Joined successfully</color>");
                networkMatch.matchId = _matchID.ToGuid ();
                TargetJoinGame (true, _matchID, playerIndex);
            } else {
                Debug.Log ($"<color = red>Game Joined failed</color>");
                TargetJoinGame (false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetJoinGame (bool success, string _matchID, int _playerIndex) {
            playerIndex = _playerIndex;
            matchID = _matchID;
            Debug.Log ($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.JoinSuccess (success, _matchID);
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
                Debug.Log ($"<color = green>Game Joined successfully</color>");
                networkMatch.matchId = matchID.ToGuid ();
                TargetSearchGame (true, matchID, playerIndex);
            } else {
                Debug.Log ($"<color = red>Game Joined failed</color>");
                TargetSearchGame (false, matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetSearchGame (bool success, string _matchID, int _playerIndex) {
            playerIndex = _playerIndex;
            matchID = _matchID;
            Debug.Log ($"MatchID: {matchID} == {_matchID} | {success}");
            UILobby.instance.SearchGameSuccess (success);
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
            Debug.Log ($"<color = red>Game Beginning</color>");
        }

        public void StartGame () { //Server
            TargetBeginGame ();
            RpcBeginGame (currentMatch.players.Count); //Sit elsewhere
        }

        [TargetRpc]
        void TargetBeginGame () {
            Debug.Log ($"MatchID: {matchID} | Beginning");
            //Additively load game scene
            SceneManager.LoadScene (2, LoadSceneMode.Additive);
            uIPlayerHand.handTransform.gameObject.SetActive (true);
        }

        [ClientRpc]
        void RpcBeginGame (int playerCount) {
            if (!isLocalPlayer) {
                uIPlayerHand.SitDown (playerIndex, playerCount);
                uIPlayerHand.handTransform.gameObject.SetActive (false);
            }
        }

        //Receiving Cards
        public void ReceiveCard (Card card) {
            //Server
            hand.Add (card);
            RpcReceiveCard (card); //all clients
        }

        [ClientRpc]
        void RpcReceiveCard (Card card) {
            if (isLocalPlayer) {
                // Debug.Log ($"Received Card: {card.suit} {card.value}");

                GameObject newCard = Instantiate (cardPrefab, uIPlayerHand.handTransform);
                UICard uiCard = newCard.GetComponent<UICard> ();
                uiCard.SetCard (card);
                uiCard.SetUIPlayerHand (uIPlayerHand);
            } else {
                // Debug.Log ($"Received Card: {card.suit} {card.value}");

                uIPlayerHand.SpawnCardBack ();

            }
        }

        public void PlaceCardOnTable (Card card) {
            CmdPlaceCardOnTable (card);
        }

        [Command]
        void CmdPlaceCardOnTable (Card card) {
            TurnManager.instance.PlaceCardOnTable (card);
        }

        public void PlacedCardOnTable (Card card) {
            TargetPlacedCardOnTable (card);
        }

        [TargetRpc]
        void TargetPlacedCardOnTable (Card card) {
            GameObject newCard = Instantiate (cardPrefab, uIPlayerHand.tableHandTransform);
            UICard uiCard = newCard.GetComponent<UICard> ();
            uiCard.SetCard (card);
            uiCard.SetUIPlayerHand (uIPlayerHand);
        }

    }

}