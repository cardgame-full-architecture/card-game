using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RandomPosition : NetworkBehaviour {

    new Camera camera;
    [SerializeField] SpriteRenderer icon;
    [SerializeField] TrailRenderer trail;

    [SerializeField, SyncVar] Vector3 colorVector;
    Color playerColour;

    void Start () {
        if (isLocalPlayer) {
            playerColour = GetRandomColor ();
            
            colorVector = new Vector3 (playerColour.r, playerColour.g, playerColour.b);
            trail.startColor = playerColour;
            trail.endColor = playerColour;
            icon.color = playerColour;
            CmdSetColor (colorVector);

            camera = Camera.main;
            StartCoroutine (UpdatePosition ());
        } else {
            SetColor (colorVector);
        }
    }

    [Command]
    void CmdSetColor (Vector3 colorVector) {
        this.colorVector = colorVector;
        SetColor (colorVector);
        Debug.Log ($"Setting color on server {colorVector}");

        RpcSetColor (colorVector);
    }

    [ClientRpc]
    void RpcSetColor (Vector3 colorVector) {
        this.colorVector = colorVector;
        SetColor (colorVector);
        Debug.Log ($"Setting color on other client {colorVector}");
    }

    void SetColor (Vector3 colorVector) {
        playerColour = new Color (colorVector.x, colorVector.y, colorVector.z);
        trail.startColor = playerColour;
        trail.endColor = playerColour;
        icon.color = playerColour;
    }

    Color GetRandomColor () {
        Color _playerColour = new Color (Random.Range (0.2f, 0.8f), 0, 0);
        while (_playerColour.g == 0) {
            float random = Random.Range (0.2f, 0.8f);
            if (Mathf.Abs (random - _playerColour.r) >= 0.2f) {
                _playerColour.g = random;
            }
        }
        while (_playerColour.b == 0) {
            float random = Random.Range (0.2f, 0.8f);
            if (Mathf.Abs (random - _playerColour.g) >= 0.2f) {
                _playerColour.b = random;
            }
        }

        return _playerColour;
    }

    IEnumerator UpdatePosition () {
        float counter = 1;
        while (true) {
            if (OnScreen (Input.mousePosition)) {
                Cursor.visible = false;
                Vector3 worldPos = camera.ScreenToWorldPoint (Input.mousePosition);
                worldPos.z = transform.position.z;
                transform.position = worldPos;
                yield return null;
            } else {
                Cursor.visible = true;
                yield return null;
            }
        }
    }

    bool OnScreen (Vector2 mousePosition) {
        Vector2 viewPortPoint = camera.ScreenToViewportPoint (mousePosition);
        return (viewPortPoint.x >= 0 && viewPortPoint.x <= 1 && viewPortPoint.y >= 0 && viewPortPoint.y <= 1);
    }
}