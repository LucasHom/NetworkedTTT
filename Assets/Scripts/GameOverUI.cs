using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextGUI;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Color tieColor;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
            Hide();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        GameManager.Instance.OnGameTied += GameManager_OnGameTied;
        Hide();
    }

    private void GameManager_OnGameTied(object sender, EventArgs e)
    {
        resultTextGUI.text = "TIE!";
        resultTextGUI.color = tieColor;
        Show();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
    }


    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            resultTextGUI.text = "YOU WIN!";
            resultTextGUI.color = winColor;
        }
        else
        {
            resultTextGUI.text = "YOU LOSE!";
            resultTextGUI.color = loseColor;
        }
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
