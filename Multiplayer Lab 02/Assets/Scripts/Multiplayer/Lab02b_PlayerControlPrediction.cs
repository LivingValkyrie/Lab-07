using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

/// <summary>
/// Author: Matt Gipson
/// Contact: Deadwynn@gmail.com
/// Domain: www.livingvalkyrie.com
/// 
/// Description: Lab02b_PlayerControlPrediction 
/// </summary>
public class Lab02b_PlayerControlPrediction : NetworkBehaviour {
    #region Fields

    [SyncVar(hook = "OnServerStateChanged")]
    PlayerState serverState;
    PlayerState predictedState;

    Queue<KeyCode> pendingMoves; 

    #endregion

    // Use this for initialization
    void Start() {
        InitState();
        predictedState = serverState;
        if (isLocalPlayer) {
            pendingMoves = new Queue<KeyCode>();
            UpdatePredictedState();
        }
        SyncState();
    }

    // Update is called once per frame
    void Update() {
        if ( isLocalPlayer ) {
            print("Pending moves: " + pendingMoves.Count);

            KeyCode[] possibleKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.Space };
            foreach ( KeyCode possibleKey in possibleKeys ) {
                //if ( !Input.GetKey( possibleKey ) ) {
                //    continue;
                //}
                //CmdMoveOnServer( possibleKey );

                if ( Input.GetKey( possibleKey ) ) {
                    CmdMoveOnServer( possibleKey );
                }

                pendingMoves.Enqueue(possibleKey);
                UpdatePredictedState();
                CmdMoveOnServer(possibleKey);
            }

        }

        SyncState();
       
    }

    [Command]
    void CmdMoveOnServer( KeyCode pressedKey ) {
        serverState = Move(serverState, pressedKey);
    }

    PlayerState Move( PlayerState previous, KeyCode newKey ) {
        float deltaX = 0, deltaY = 0, deltaZ = 0;
        float deltaRotationY = 0;

        switch ( newKey ) {
            case KeyCode.Q:
                deltaX = -0.5f;
                break;
            case KeyCode.S:
                deltaZ = -0.5f;
                break;
            case KeyCode.E:
                deltaX = 0.5f;
                break;
            case KeyCode.W:
                deltaZ = 0.5f;
                break;
            case KeyCode.A:
                deltaRotationY = -1f;
                break;
            case KeyCode.D:
                deltaRotationY = 1f;
                break;
        }

        return new PlayerState {
            movementNumber = 1 + previous.movementNumber,
            posX = deltaX + previous.posX,
            posY = deltaY + previous.posY,
            posZ = deltaZ + previous.posZ,
            rotX = previous.rotX,
            rotY = deltaRotationY + previous.rotY,
            rotZ = previous.rotZ
        };
    }

    void SyncState() {
        PlayerState stateToRender = isLocalPlayer ? predictedState : serverState;

        transform.position = new Vector3(stateToRender.posX, stateToRender.posY, stateToRender.posZ);
        transform.rotation = Quaternion.Euler(stateToRender.rotX, stateToRender.rotY, stateToRender.rotZ);
    }

    void OnServerStateChanged(PlayerState newState) {
        serverState = newState;
        if (pendingMoves != null) {
            while (pendingMoves.Count > (predictedState.movementNumber - serverState.movementNumber)) {
                pendingMoves.Dequeue();
            }
            UpdatePredictedState();
        }
    }

    void UpdatePredictedState() {
        predictedState = serverState;
        foreach (KeyCode pendingMove in pendingMoves) {
            predictedState = Move(predictedState, pendingMove);
        }
    }

    [Server]
    void InitState() {
        serverState = new PlayerState {
            posX = -119f,
            posY = 165.08f,
            posZ = -924f,
            rotX = 0f,
            rotY = 0f,
            rotZ = 0f
        };
    }
}