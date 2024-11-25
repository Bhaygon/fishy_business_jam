using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public GameObject DeathCanvas;
    public GameObject WinCanvas;
    public Button RestartButton;
    public Button WinButton;
    public Transform HealthHolder;
    public TMP_Text PearlAmountText;
    private int _pearlAmount;
    private bool _ended = false;

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
            GetComponent<SoundManager>().StartGameAudio();
        }
        
        StartGameplay();
    }

    private void OnEnable()
    {
        RestartButton.onClick.AddListener(RestartScene);
        WinButton.onClick.AddListener(RestartScene);
    }

    private void OnDisable()
    {
        RestartButton.onClick.RemoveAllListeners();
        WinButton.onClick.RemoveAllListeners();
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartGameplay();
    }

    public void AddPearl()
    {
        _pearlAmount++;
        PearlAmountText.text = _pearlAmount.ToString();
    }

    private void StartGameplay()
    {
        _ended = false;
        _pearlAmount = 0;
        PearlAmountText.text = _pearlAmount.ToString();
        DeathCanvas.SetActive(false);
        WinCanvas.SetActive(false);
        GetComponent<SoundManager>().StartGameAudio();
    }

    public void ShowDeathScreen()
    {
        if (_ended) return;
        _ended = true;
        DeathCanvas.SetActive(true);
        GetComponent<SoundManager>().GameOverSound();
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

    public void BossKilled()
    {
        if (_ended) return;
        _ended = true;
        StartCoroutine(OnWin());
        
    }

    public IEnumerator OnWin()
    {
        yield return new WaitForSeconds(0.5f);
        WinCanvas.SetActive(true);
    }
}
