using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkStatus : NetworkBehaviour {
    public override void OnStartClient () {
        Debug.Log (Transport.activeTransport.ServerGetClientAddress (connectionToClient.connectionId));
    }
}