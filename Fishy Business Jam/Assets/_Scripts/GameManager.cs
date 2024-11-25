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
    private int _score;
    public TMP_Text ScoretText;
    private Coroutine _timerCo;
    private SoundManager _soundManager;


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

        _soundManager = GetComponent<SoundManager>();
        
        StartGameplay();
    }

    public void PlaySFX(AudioClip clip)
    {
        _soundManager.sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        _soundManager.musicSource.clip = clip;
        _soundManager.musicSource.Play();
    }

    private IEnumerator TimeScore()
    {
        int count = 0;
        while (_ended == false)
        {
            yield return new WaitForSeconds(3);
            AddScore(-1);
            //("reduced 1 point, total: " + _score);
            count++;
            if (count > 40) break;
        }
    }

    public void AddScore(int score)
    {
        _score += score;
        if (_score < 0) score = 0;
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
        AddScore(50);
        PearlAmountText.text = _pearlAmount.ToString();
    }

    private void StartGameplay()
    {
        _ended = false;
        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = StartCoroutine(TimeScore());
        _score = 300;
        _pearlAmount = 0;
        PearlAmountText.text = _pearlAmount.ToString();
        DeathCanvas.SetActive(false);
        WinCanvas.SetActive(false);
        _soundManager.StartGameAudio();
    }

    public void ShowDeathScreen()
    {
        if (_ended) return;
        _ended = true;
        DeathCanvas.SetActive(true);
        _soundManager.GameOverSound();
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
        GameObject.Find("PLAYER").SetActive(false);
        ScoretText.text = "Your final score is " + _score + " points \nCollect pearls, kill enemies and do everything faster to increase your score!";
        WinCanvas.SetActive(true);
    }
}
