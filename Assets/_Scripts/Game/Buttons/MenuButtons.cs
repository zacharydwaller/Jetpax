using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class MenuButtons : MonoBehaviour
{
    public GameManager gameManager;

    // Change number of players
    public void setNumPlayers(float newNumPlayers)
    {
        gameManager.numPlayers = (int) newNumPlayers;
    }

    // Change competitive mode
    public void setCompetitive(bool newIsCompetitive)
    {
        gameManager.isCompetitiveGame = newIsCompetitive;
    }

    // Loads a level
    public void gotoLevel(string level)
    {
        SceneManager.LoadScene(level);
    }

    public void SinglePlayer()
    {
        SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        NetworkManager.singleton.StartHost();
    }

    // Exit game
    public void exitGame()
    {
        Application.Quit();
    }
}
