using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{

    private const float GRID_SIZE = 3.1f;

    [SerializeField] private GameObject crossPrefab;
    [SerializeField] private GameObject circlePrefab;


    private void Start()
    {
        GameManager.Instance.OnClickedGridPosition += GameManager_OnClickedGridPosition;
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
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
