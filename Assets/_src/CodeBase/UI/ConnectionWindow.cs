using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using _src.CodeBase.GameLogic;
using Assets.Scripts.Consul;
using kcp2k;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _src.CodeBase.UI
{
    public class ConnectionWindow : MonoBehaviour
    {
        [SerializeField] 
        private NetworkManager _networkManager;


        [SerializeField] 
        private Transform _connectionPanel;
        
        
        [SerializeField] 
        private InputField _playerNameInputField;


        [SerializeField] 
        private InputField _roomIdCodeInputField;


        [SerializeField] 
        private Button _joinButton;


        [SerializeField] 
        private Button _hostButton;


        [SerializeField] 
        private Canvas _searchingCanvas;


        private async void Start()
        {
            if (Application.isBatchMode)
                return;
            
            
            _joinButton.onClick.AddListener(OnClickJoinButton);
            _hostButton.onClick.AddListener(OnClickHostButton);

            StartCoroutine(TestRoutine());
        }

        private IEnumerator TestRoutine()
        {
            for (int i = 0; i < 3; i++)
            {
                Debug.Log("try connect");
                if (CheckRoomAvailable().GetAwaiter().GetResult() is GameStateData gameStateData)
                {
                    _connectionPanel.gameObject.SetActive(false);

                    yield return StartCoroutine(ConnectWithDelay(gameStateData));
                }     
            }
            
            _connectionPanel.gameObject.SetActive(true);
            _searchingCanvas.enabled = false;
        }

        // private async void OnClientDisconnected(int obj)
        // {
        //     Debug.Log("DISCONNECTED");
        //     
        //     if (await CheckRoomAvailable() is GameStateData gameStateData)
        //     {
        //         _connectionPanel.gameObject.SetActive(false);
        //
        //         StartCoroutine(ConnectWithDelay(gameStateData));
        //     }
        // }

        private IEnumerator ConnectWithDelay(GameStateData gameStateData)
        {
            yield return new WaitForSeconds(7);
            
            PlayerPrefs.SetInt("IsCrashed", 1);
            PlayerPrefs.SetString("GameData", JsonConvert.SerializeObject(gameStateData));
                
            JoinToNeededServer(PlayerPrefs.GetString("RoomId"), gameStateData.ServerIp);
        }

        private async Task<GameStateData> CheckRoomAvailable()
        {
            GameStateData gameStateData;
            
            string roomId = PlayerPrefs.GetString("RoomId");
            
            ConsulClient consulClient = new ConsulClient();
            KVPair kvPair = await consulClient.GetKV(roomId);
            
            if (kvPair == null)
                return null;

            gameStateData = JsonConvert.DeserializeObject<GameStateData>(Encoding.UTF8.GetString(kvPair.Value));
            
            Debug.Log($"ip in new server {gameStateData.ServerIp}. First user {gameStateData.ClientDatas[0].Score}");
            
            return gameStateData;
        }

        private void OnClickJoinButton()
        {
            string playerName = _playerNameInputField.text;
            string roomId = _roomIdCodeInputField.text.ToUpper();
            
            if (playerName.Trim() == String.Empty || roomId.Trim() == String.Empty)
                return;
            
            
            PlayerPrefs.SetString("Name", playerName);
            PlayerPrefs.SetInt("IsHost", 0);
            PlayerPrefs.SetString("RoomId", roomId);

            JoinToNeededServer(roomId);
        }

        private async void JoinToNeededServer(string roomId, string serverIp = "")
        {
            ConsulClient consulClient = new ConsulClient();
            KVPair kvPair = await consulClient.GetKV(roomId);
            
            if (kvPair == null)
                return;
            
            ActivateSearching();
            
            GameStateData gameStateData = JsonConvert.DeserializeObject<GameStateData>(Encoding.UTF8.GetString(kvPair.Value));
            
            Debug.Log($"Connecting to {roomId} on {gameStateData.ServerIp} address");
            
            _networkManager.networkAddress = gameStateData.ServerIp;
            _networkManager.StartClient();
            // Debug.Log($"{res.Key}: {Encoding.UTF8.GetString(res.Value)}");
        }

        private void OnClickHostButton()
        {
            string playerName = _playerNameInputField.text;
            if (playerName.Trim() == String.Empty)
                return;

            
            PlayerPrefs.SetString("Name", playerName);
            PlayerPrefs.SetInt("IsHost", 1);

            CreateRoomOnServer();
        }

        private void ActivateSearching()
        {
            _searchingCanvas.enabled = true;
        }

        private async void CreateRoomOnServer()
        {
            string availableGameServerIp = await GetAvailableGameServerIp();

            if (availableGameServerIp == String.Empty)
            {
                Debug.Log("Available server not exist");
                return;
            }
            
            ActivateSearching();
            
            Debug.Log($"Creating room on {availableGameServerIp}");

            _networkManager.networkAddress = availableGameServerIp;
            _networkManager.StartClient();
        }

        private async Task<string> GetAvailableGameServerIp()
        {
            ConsulClient consulClient = new ConsulClient();
            ServiceEntry[] activeServices = await consulClient.GetAliveServiceEntries("unityclient");

            if (activeServices == null || activeServices.Length <= 0)
                return "";
            
            return activeServices[Random.Range(0, activeServices.Length)].Node.Address;
        }
    }
}