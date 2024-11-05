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

    void Start()
    {
        clearMindSpawnSlider.minValue = gameManager.clearMindSpawnCount;
        infectedSpawnSlider.minValue = gameManager.infectedSpawnCount;
        clearMindSpawnSlider.value = clearMindSpawnSlider.minValue;
        infectedSpawnSlider.value = infectedSpawnSlider.minValue;
    }
    // Update is called once per frame
    void Update()
    {
        UpdateText();
        UpdateSlider();
    }

    public void QuitGame()
    {
        //Debug line to test quit function in editor
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    void UpdateSlider()
    {
        gameManager.clearMindSpawnCount = clearMindSpawnSlider.value;
        gameManager.infectedSpawnCount = infectedSpawnSlider.value;
        clearMindSliderText.text = string.Format("Clear Mind Spawn Count = {0}", gameManager.clearMindSpawnCount);
        infectedSliderText.text = string.Format("Infected Spawn Count = {0}", gameManager.infectedSpawnCount);
    }
    void UpdateText()
    {
        infectedPopText.text = string.Format("INFECTED POPULATION = {0}", gameManager.hivePop.Count);
        healthPopText.text = string.Format("HEALTHY POPULATION = {0}", gameManager.genPop.Count);
    }

    public void ReloadScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("World");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
