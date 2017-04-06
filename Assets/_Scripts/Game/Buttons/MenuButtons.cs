using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuButtons : MonoBehaviour
{
    public Game_Manager gameManager;

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
        SceneManager.LoadScene("Level1");
        // network manager host lan game
    }

    // Exit game
    public void exitGame()
    {
        Application.Quit();
    }
}
