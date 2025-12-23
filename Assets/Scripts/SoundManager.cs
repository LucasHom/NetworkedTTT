using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const float SFX_LIFETIME = 5f;
    [SerializeField] private GameObject placeSFXPrefab;
    [SerializeField] private GameObject winSFXPrefab;
    [SerializeField] private GameObject loseSFXPrefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            GameObject sfxGameObject = Instantiate(winSFXPrefab);
            Destroy(sfxGameObject, SFX_LIFETIME);
        }
        else
        {
            GameObject sfxGameObject = Instantiate(loseSFXPrefab);
            Destroy(sfxGameObject, SFX_LIFETIME);
        }
    }

    private void GameManager_OnPlacedObject(object sender, EventArgs e)
    {
        GameObject sfxGameObject = Instantiate(placeSFXPrefab);
        Destroy(sfxGameObject, SFX_LIFETIME);
    }
}
