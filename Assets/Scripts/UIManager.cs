using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;
    public TextMeshProUGUI healthPopText;
    public TextMeshProUGUI infectedPopText;
    public TextMeshProUGUI clearMindSliderText;
    public TextMeshProUGUI infectedSliderText;
    public Slider clearMindSpawnSlider;
    public Slider infectedSpawnSlider;
    void Awake()
    {
        if(gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    public void QuitGame()
    {
        //Debug line to test quit function in editor
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void ReloadScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("World");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameManager.ClearPopulation();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
