using Mirror;
using UnityEngine;

namespace _src.CodeBase.Net {
    public class AutoHostClient : MonoBehaviour {

        [SerializeField] NetworkManager networkManager;

        void Start () {
            if (!Application.isBatchMode) { //Headless build
                Debug.Log ($"=== Client Build ===");
                networkManager.StartClient ();
            } else {
                Debug.Log ($"=== Server Build ===");
                Debug.Log(networkManager.networkAddress);
            }
        }

        public void JoinLocal () {
            networkManager.networkAddress = "192.168.1.110";
            networkManager.StartClient ();
        }
    }
}