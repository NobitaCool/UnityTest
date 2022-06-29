using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject menuScene;
    [SerializeField] private GameObject gameOverScene;
    [SerializeField] private GenerateBoard generateBoard;
    public void StartGame()
    {
        menuScene.SetActive(false);
        generateBoard.GenerateAllTiles();
    }

    public void RestartGame()
    {
        ExitGame();
        StartGame();
    }

    public void ExitGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
