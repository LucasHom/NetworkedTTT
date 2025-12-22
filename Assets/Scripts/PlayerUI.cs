using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject crossYouTextGameObject;
    [SerializeField] private GameObject circleYouTextGameObject;
    [SerializeField] private TextMeshProUGUI playerCrossScoreTextMesh;
    [SerializeField] private TextMeshProUGUI playerCircleScoreTextMesh;

    private void Awake()
    {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);

        playerCrossScoreTextMesh.text = "0";
        playerCircleScoreTextMesh.text = "0";
    }

    private void Start()
    {
        //since both these events are invoked on the server we need them to run on server and client
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
    }

    private void GameManager_OnScoreChanged(object sender, EventArgs e)
    {
        GameManager.Instance.GetScores(out int crossScore, out int circleScore);
        playerCrossScoreTextMesh.text = crossScore.ToString();
        playerCircleScoreTextMesh.text = circleScore.ToString();
    }


    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouTextGameObject.SetActive(true);
        }
        else
        {
            circleYouTextGameObject.SetActive(true);
        }

        playerCrossScoreTextMesh.text = "0";
        playerCircleScoreTextMesh.text = "0";

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        }
        else
        {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(true);
        }

    }
}
