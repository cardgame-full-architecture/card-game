﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using UnityEngine;

namespace _src.CodeBase.Net {

    [System.Serializable]
    public class Match {
        public string matchID;
        public bool publicMatch;
        public bool inMatch;
        public bool matchFull;
        public List<GameLogic.Player> players = new List<GameLogic.Player> ();

        public Match (string matchID, GameLogic.Player player, bool publicMatch) {
            matchFull = false;
            inMatch = false;
            this.matchID = matchID;
            this.publicMatch = publicMatch;
            players.Add (player);
        }

        public Match () { }
    }

    public class MatchMaker : NetworkBehaviour {

        public static MatchMaker instance;

        public SyncList<Match> matches = new SyncList<Match> ();
        public SyncList<String> matchIDs = new SyncList<String> ();

        [SerializeField] GameObject turnManagerPrefab;
        [SerializeField] int maxMatchPlayers = 12;

        public event Action<GameLogic.Player> OnPlayerDisconnected; 

        void Start () {
            instance = this;
        }

        public bool HostGame(string _matchID, GameLogic.Player _player, bool publicMatch, out int playerIndex, string playerName) {
            playerIndex = -1;

            if (!matchIDs.Contains (_matchID)) {
                matchIDs.Add (_matchID);
                Match match = new Match (_matchID, _player, publicMatch);
                matches.Add (match);
                Debug.Log ($"Match generated");
                _player.currentMatch = match;
                _player.PlayerName = playerName;
                playerIndex = 1;
                return true;
            } else {
                Debug.Log ($"Match ID already exists");
                return false;
            }
        }

        public bool JoinGame(string _matchID, GameLogic.Player player, out int playerIndex, string playerName) {
            playerIndex = -1;

            if (matchIDs.Contains (_matchID)) {

                for (int i = 0; i < matches.Count; i++) {
                    if (matches[i].matchID == _matchID) {
                        if (!matches[i].inMatch && !matches[i].matchFull) {
                            matches[i].players.Add (player);
                            player.currentMatch = matches[i];
                            player.PlayerName = playerName;
                            playerIndex = matches[i].players.Count;

                            if (matches[i].players.Count == maxMatchPlayers) {
                                matches[i].matchFull = true;
                            }

                            break;
                        } else {
                            return false;
                        }
                    }
                }

                Debug.Log ($"Match joined");
                return true;
            } else {
                Debug.Log ($"Match ID does not exist");
                return false;
            }
        }

        public bool SearchGame (GameLogic.Player _player, out int playerIndex, out string matchID) {
            playerIndex = -1;
            matchID = "";

            for (int i = 0; i < matches.Count; i++) {
                Debug.Log ($"Checking match {matches[i].matchID} | inMatch {matches[i].inMatch} | matchFull {matches[i].matchFull} | publicMatch {matches[i].publicMatch}");
                if (!matches[i].inMatch && !matches[i].matchFull && matches[i].publicMatch) {
                    if (JoinGame (matches[i].matchID, _player, out playerIndex, "some name")) {
                        matchID = matches[i].matchID;
                        return true;
                    }
                }
            }

            return false;
        }

        public void BeginGame (string _matchID) {
            GameLogic.TurnManager turnManager = Instantiate (turnManagerPrefab).GetComponent<GameLogic.TurnManager> ();
            // GameObject newTurnManager = Instantiate (turnManagerPrefab);
            // NetworkServer.Spawn (newTurnManager);
            // newTurnManager.GetComponent<NetworkMatch> ().matchId = _matchID.ToGuid ();
            // TurnManager turnManager = newTurnManager.GetComponent<TurnManager> ();

            for (int i = 0; i < matches.Count; i++) {
                if (matches[i].matchID == _matchID) {
                    matches[i].inMatch = true;

                    List<GameLogic.Player> players = new List<GameLogic.Player>();

                    foreach (var player in matches[i].players) {
                        player.StartGame ();

                        players.Add(player);
                    }
                    
                    turnManager.ManagePlayers(players, this);
                    break;
                }
            }
        }

        public static string GetRandomMatchID () {
            string _id = string.Empty;
            for (int i = 0; i < 5; i++) {
                int random = UnityEngine.Random.Range (0, 36);
                if (random < 26) {
                    _id += (char) (random + 65);
                } else {
                    _id += (random - 26).ToString ();
                }
            }
            Debug.Log ($"Random Match ID: {_id}");
            return _id;
        }

        public void PlayerDisconnected (GameLogic.Player player, string _matchID) {
            for (int i = 0; i < matches.Count; i++) {
                if (matches[i].matchID == _matchID) {
                    int playerIndex = matches[i].players.IndexOf (player);
                    matches[i].players.RemoveAt (playerIndex);
                    OnPlayerDisconnected?.Invoke(player);
                    Debug.Log ($"Player disconnected from match {_matchID} | {matches[i].players.Count} players remaining");

                    if (matches[i].players.Count == 0) {
                        Debug.Log ($"No more players in Match. Terminating {_matchID}");
                        matches.RemoveAt (i);
                        matchIDs.Remove (_matchID);
                    }
                    break;
                }
            }
        }

    }

    public static class MatchExtensions {
        public static Guid ToGuid (this string id) {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider ();
            byte[] inputBytes = Encoding.Default.GetBytes (id);
            byte[] hashBytes = provider.ComputeHash (inputBytes);

            return new Guid (hashBytes);
        }
    }

}