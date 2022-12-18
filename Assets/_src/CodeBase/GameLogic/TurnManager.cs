using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _src.CodeBase.Data;
using _src.CodeBase.Net;
using Assets.Scripts.Consul;
using Newtonsoft.Json;
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
        private ConsulClient _consulClient;
        
        private int _countOfAnsweredUsers;
        private Dictionary<Player, string> _playersMessages;

        private int _countOfUsersVotes;
        private Dictionary<Player, int> _playersScores;

        private GameStateData _gameStateData;
        private string _matchId;

        void Start () {
            instance = this;
        }

        private void OnDestroy()
        {
            _matchMaker.OnPlayerDisconnected -= OnPlayerDisconnected;
        }

        public void ManagePlayers(List<Player> _players, MatchMaker matchMaker, string matchId, string serverAddress) 
        {
            players = _players;
            _matchId = matchId;

            _matchMaker = matchMaker;
            _matchMaker.OnPlayerDisconnected += OnPlayerDisconnected;

            _gameStateData = new GameStateData(serverAddress, _players);
            _consulClient = new ConsulClient();
            _consulClient.SetKV(_matchId, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_gameStateData))).Wait();

            StartCoroutine(GameLoopRoutine());
        }

        public void OnMessageSent(Player player, string message)
        {
            _countOfAnsweredUsers++;
            _playersMessages.Add(player, message);
            player.AcceptAnswer();
        }
        
        public void SelectVariant(string variantText)
        {
            _countOfUsersVotes++;

            foreach (KeyValuePair<Player,string> playersMessage in _playersMessages)
            {
                if (playersMessage.Value == variantText)
                {
                    if (_playersScores.ContainsKey(playersMessage.Key))
                        _playersScores[playersMessage.Key]++;
                    else
                        _playersScores.Add(playersMessage.Key, 1);
                }
            }
        }

        private IEnumerator GameLoopRoutine()
        {
            _playersScores = new Dictionary<Player, int>();
            foreach (Player player in players) 
                player.ActivatePlayerScore();

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
            _countOfUsersVotes = 0;
            
            foreach (Player player in players)
            {
                foreach (KeyValuePair<Player, string> playersMessage in _playersMessages)
                {
                    if (player != playersMessage.Key)
                        player.AddUserVariant(playersMessage.Value);   
                }
            }
            
            while (_countOfUsersVotes < players.Count)
                yield return new WaitForSeconds(.5f);

            foreach (Player player in players)
            {
                if (_playersScores.ContainsKey(player)) 
                    player.UpdateScore(_playersScores[player]);
                else
                    player.UpdateScore(0);
            }

            foreach (ClientData clientData in _gameStateData.ClientDatas)
            {
                foreach (KeyValuePair<Player,int> playersScore in _playersScores)
                {
                    if (playersScore.Key.PlayerName == clientData.Name) 
                        clientData.Score = playersScore.Value;
                }
            }
            
            if (players.Count <= 0)
                yield break;
            
            _consulClient.SetKV(_matchId, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_gameStateData))).Wait();
        }

        private void OnPlayerDisconnected(Player player)
        {
            if (!players.Contains(player))
                return;


            players.Remove(player);
        }
    }
}