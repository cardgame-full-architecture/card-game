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
            }
        }

        public void JoinLocal () {
            networkManager.networkAddress = "localhost";
            networkManager.StartClient ();
        }
    }
}