using Mirror;
using UnityEngine;

public class HelloWorld : NetworkBehaviour {

    void Start () {
        if (isLocalPlayer) {
            Debug.Log ($"Hello Local Player");

            CmdTellServerToDoSomething ();
        }

        if (isServer) {
            Debug.Log ($"Hello Server");
        }

        if (isClientOnly && !isLocalPlayer) {
            Debug.Log ($"Hello Client");
        }
    }

    [Command] //This is called from a client
    void CmdTellServerToDoSomething () {
        //You can use singletons to access managers on the server (eg: TurnManager)
        Debug.Log ($"Server doing something");

        RpcTellClientsToDoSomething ();

        TargetTellOwnerClientToDoSomething ();
    }

    [ClientRpc] //This is called from a server
    void RpcTellClientsToDoSomething () {
        //This could be used to spawn a players cards on all clients (eg: flipping a card over)
        Debug.Log ($"All clients doing something. Local Player: {isLocalPlayer}");
    }

    [TargetRpc] //This is called from a server
    void TargetTellOwnerClientToDoSomething () {
        //This could be used to deal a card directly to the local player and noot all clients
        Debug.Log ($"Only local client doing something. Local Player: {isLocalPlayer}");
    }

}