using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class GameVisualManager : NetworkBehaviour
{

    private const float GRID_SIZE = 3.1f;

    [SerializeField] private GameObject crossPrefab;
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private GameObject lineCompletePrefab;


    private List<GameObject> visualGameObjectList;


    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }
        visualGameObjectList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        float eulerZ = 0f;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = -45f;
                break;
        }
        GameObject lineCompleteGameObject = Instantiate(lineCompletePrefab, GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        lineCompleteGameObject.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(lineCompleteGameObject);
    }


    //Subscriber to the OnClickedGridPosition event
    private void GameManager_OnClickedGridPosition(object sender, GameManager.OnClickedGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    //Gonna make this an RPC(Remote Procedure Call) so that all clients spawn object throguh the network
    [Rpc(SendTo.Server)] //this func is run on the server
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        GameObject prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }
        GameObject spawnedCrossObject = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        //says that this obj should be spawned across all clients
        spawnedCrossObject.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(spawnedCrossObject);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
