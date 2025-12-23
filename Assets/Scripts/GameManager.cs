using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static GameManager;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    //events
    public event EventHandler<OnClickedGridPositionEventArgs> OnClickedGridPosition;
    public class OnClickedGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;

    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
        public string winnerTask;
        public string loserTask;
    }

    public event EventHandler OnRematch;
    public event EventHandler OnGameTied;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnPlacedObject;


    //Best to use this to track player instead of having different string trackers
    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;

    }

    private PlayerType localPlayerType;
    //network var of type player type must be initialized
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;
    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();

    // Tasks for the losing player
    public List<string> loserTasks = new List<string>()
{
    "Close your eyes",
    "Do 5 pushups",
    "If you blink, redo",
    "Wait to be put in position, then don't move for the next minute",
    "Wait to be spun, then walk in a straight line",
    "Don't move",
    "The winner will whisper you a secret word. Stay silent until you hear it again.",
    "Spell SeanShawnShaun backwards under 30 seconds",
    "Arm wrestle the winner but purposely lose (make it close)",
    "This all means nothing. it's a distraction! But now... grab the nearest purple object. Slowest does 10 pushups.",
    "Close your eyes"
};

    // Tasks for the winning player (corresponding to the loserTasks)
    public List<string> winnerTasks = new List<string>()
{
    "Pour water on the loser's head",
    "Sit on the loser’s back",
    "Flick the loser’s forehead",
    "Change the loser’s position; they must keep it for 1 minute",
    "Spin the loser 15 times",
    "Stack 3 items on the loser’s head",
    "Whisper the loser a word",
    "Start a 30 second timer; if they fail to spell SeanShawnShaun backwards, pour water on their head",
    "Arm wrestle the loser",
    "Grab the nearest purple object. Slowest does 10 pushups.",
    "Use a snapchat filter on the opponent"
};


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one GameManager instance!");
        }
        Instance = this;

        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>
        {
            //Horitonzal lines
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
                centerGridPosition = new Vector2Int(1,0),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2) },
                centerGridPosition = new Vector2Int(1,2),
                orientation = Orientation.Horizontal
            },

            //Vertical lines
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2) },
                centerGridPosition = new Vector2Int(0,1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2) },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2) },
                centerGridPosition = new Vector2Int(2,1),
                orientation = Orientation.Vertical
            },

            //Diagonal lines
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2) },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalA
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0) },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalB
            }
        };
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            //when clients have connected to server 
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        //subscribing to value changed event with my custom func
        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCrossScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCircleScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    //only runs on server , meaning currentPlayablePlayerType only correct over the server
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            return;
        }

        playerTypeArray[x, y] = playerType;
        TriggerOnPlacedObjectRpc();

        //call funcs subbed to event
        OnClickedGridPosition?.Invoke(this, new OnClickedGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        switch (currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlacedObjectRpc()
    {
        OnPlacedObject?.Invoke(this, EventArgs.Empty);
    }

    private bool TestWinnerLine(PlayerType a, PlayerType b, PlayerType c)
    {
        return a != PlayerType.None && a == b && b == c;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private void TestWinner()
    {
        for ( int i = 0; i<lineList.Count; i++)
        {
            Line line = lineList[i];
            if (TestWinnerLine(line))
            {
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType = playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y];
                switch (winPlayerType)
                {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;
                    case PlayerType.Circle:
                        playerCircleScore.Value++;
                        break;
                }
                int maxIndex = Mathf.Min(loserTasks.Count, winnerTasks.Count);
                int randIndex = UnityEngine.Random.Range(0, maxIndex);

                TriggerOnGameWinRpc(i, winPlayerType, winnerTasks[randIndex], loserTasks[randIndex]);
                return;
            }
        }

        bool hasTie = true;
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                {
                    hasTie = false;
                    break;
                }
            }
        }

        if (hasTie)
        {
            TriggerOnGameTiedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRpc()
    {
        OnGameTied?.Invoke(this, EventArgs.Empty);
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType, string winnerTask, string loserTask)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = line,
            winPlayerType = winPlayerType,
            winnerTask = winnerTask,
            loserTask = loserTask
        });
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }
        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }


    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    public void GetScores(out int crossScore, out int circleScore)
    {
        crossScore = playerCrossScore.Value;
        circleScore = playerCircleScore.Value;
    }
}
