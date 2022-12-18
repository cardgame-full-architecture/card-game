using System.Net;
using Assets.Scripts.Consul;
using Mirror;
using UnityEngine;

namespace _src.CodeBase.Net {
    public class AutoHostClient : MonoBehaviour {

        [SerializeField] NetworkManager networkManager;

        async void Start () {
            if (!Application.isBatchMode) { //Headless build
                // Debug.Log ($"=== Client Build ===");
                //
                // ConsulClient consulClient = new ConsulClient();
                // foreach (ServiceEntry item in await consulClient.GetAliveServiceEntries("unityclient"))
                //     Debug.Log($"{item.Node.Name}: {item.Node.Address}, {item.Service.Address}");
                
                // networkManager.StartClient ();
            } else {
                Debug.Log ($"=== Server Build ===");
                Debug.Log($"Server started on {networkManager.networkAddress}");
                
                ConsulClient consulClient = new ConsulClient("unityclient", "unityclient1", IPAddress.Parse(networkManager.networkAddress), 7777, 0);
                if (await consulClient.RegistrationAsync())
                {
                    consulClient.StartPingTask();
                }
            }
        }

        public void JoinLocal () {
            networkManager.networkAddress = "192.168.1.110";
            networkManager.StartClient ();
        }
    }
}