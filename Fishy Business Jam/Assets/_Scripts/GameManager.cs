using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public GameObject DeathCanvas;
    public Button RestartButton;
    public Transform HealthHolder;

    private void Awake()
    {
        if (Instance)
        {
            gameObject.SetActive(false);
            Object.Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            Object.DontDestroyOnLoad(gameObject);
            SceneManager.LoadSceneAsync(1);
        }
        
        DeathCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        RestartButton.onClick.AddListener(RestartScene);
    }

    private void OnDisable()
    {
        RestartButton.onClick.RemoveAllListeners();
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        DeathCanvas.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        DeathCanvas.SetActive(true);
    }

    public void UpdateHealthUI(int currentPlayerHealth, int maxHealth)
    {
        for (int i = 0; i < 10; i++)
        {
            if (i < maxHealth) HealthHolder.GetChild(i).gameObject.SetActive(true);
            else HealthHolder.GetChild(i).gameObject.SetActive(false);
        }
        
        
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < currentPlayerHealth) HealthHolder.GetChild(i).GetComponent<Image>().color = Color.red;
            else HealthHolder.GetChild(i).GetComponent<Image>().color = Color.white;
        }
    }
}
