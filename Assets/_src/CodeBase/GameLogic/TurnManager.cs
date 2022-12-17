using System;
using System.Collections;
using System.Collections.Generic;
using _src.CodeBase.Data;
using _src.CodeBase.Net;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _src.CodeBase.GameLogic {
    public class TurnManager : MonoBehaviour {

        [SerializeField] List<Player> players;
        [SerializeField] int currentPlayerIndex;
        [SerializeField] bool gameInProgress = true;
        [SerializeField] private ImagesData _imagesData;

        public static TurnManager instance;
        private MatchMaker _matchMaker;
        private int _countOfAnsweredUsers;
        private Dictionary<Player, string> _playersMessages;

        void Start () {
            instance = this;
        }

        private void OnDestroy()
        {
            _matchMaker.OnPlayerDisconnected -= OnPlayerDisconnected;
        }

        public void ManagePlayers (List<Player> _players, MatchMaker matchMaker) {
            players = _players;

            _matchMaker = matchMaker;
            _matchMaker.OnPlayerDisconnected += OnPlayerDisconnected;

            StartCoroutine(GameLoopRoutine());
        }

        public void OnMessageSent(Player player, string message)
        {
            _countOfAnsweredUsers++;
            _playersMessages.Add(player, message);
            player.AcceptAnswer();
        }

        private IEnumerator GameLoopRoutine()
        {
            while (true)
            {
                ChangeImage();
                yield return StartCoroutine(WaitUsersMessagesRoutine());
                yield return StartCoroutine(WaitUsersVotesRoutine());
            }
        }

        private void ChangeImage()
        {
            int randomImageIndex = Random.Range(0, _imagesData.SpritesImages.Count);
                
            foreach (Player player in players)
            {
                player.SetImage(randomImageIndex);
            }
        }

        private IEnumerator WaitUsersMessagesRoutine()
        {
            _playersMessages = new Dictionary<Player, string>();
            _countOfAnsweredUsers = 0;

            foreach (Player player in players) 
                player.ActivateAnswerTools();

            while (_countOfAnsweredUsers < players.Count)
                yield return new WaitForSeconds(.5f);

            foreach (Player player in players) 
                player.DeactivateAnswering();
        }

        private IEnumerator WaitUsersVotesRoutine()
        {
            foreach (Player player in players)
            {
                foreach (KeyValuePair<Player, string> playersMessage in _playersMessages)
                {
                    if (player != playersMessage.Key)
                        player.AddUserVariant(playersMessage.Value);   
                }
            }
            
            yield return new WaitForSeconds(20);
        }

        // private IEnumerator ChangeImagesRoutine()
        // {
        //     while (gameInProgress)
        //     {
        //         int randomImageIndex = Random.Range(0, _imagesData.SpritesImages.Count);
        //         
        //         foreach (Player player in players)
        //         {
        //             player.SetImage(randomImageIndex);
        //         }
        //
        //         yield return new WaitForSeconds (1);
        //     }
        // }

        private void OnPlayerDisconnected(Player player)
        {
            if (!players.Contains(player))
                return;


            players.Remove(player);
        }
    }
}