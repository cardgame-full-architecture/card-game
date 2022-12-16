using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent (typeof (NetworkManager))]
public class NetworkManagerHelper : MonoBehaviour {

    NetworkManager networkManager;
    [SerializeField] bool serverBuild = false;

    void Start () {
        networkManager = GetComponent<NetworkManager> ();
        if (serverBuild) networkManager.StartServer ();
        else networkManager.StartClient ();
    }

    public void SetIP (string address) {
        networkManager.networkAddress = address;
    }
}