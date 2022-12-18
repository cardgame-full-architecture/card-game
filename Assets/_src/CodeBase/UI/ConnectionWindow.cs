using System;
using System.Threading.Tasks;
using Assets.Scripts.Consul;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _src.CodeBase.UI
{
    public class ConnectionWindow : MonoBehaviour
    {
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


        private void Start()
        {
            _joinButton.onClick.AddListener(OnClickJoinButton);
            _hostButton.onClick.AddListener(OnClickHostButton);
        }

        private void OnClickJoinButton()
        {
            
        }

        private void OnClickHostButton()
        {
            string playerName = _playerNameInputField.text;
            if (playerName.Trim() == String.Empty)
                return;

            ActivateSearching();

            CreateRoomOnServer();
        }

        private void ActivateSearching()
        {
            _searchingCanvas.enabled = true;
        }

        private async void CreateRoomOnServer()
        {
            string availableGameServerIp = await GetAvailableGameServerIp();
            
            Debug.Log($"Creating room on {availableGameServerIp}");
        }

        private async Task<string> GetAvailableGameServerIp()
        {
            ConsulClient consulClient = new ConsulClient();
            ServiceEntry[] activeServices = await consulClient.GetAliveServiceEntries("unityclient");
            return activeServices[Random.Range(0, activeServices.Length)].Node.Address;
        }
    }
}