using System.Collections;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    private int score;
    private bool isGameOver = false;
    private float scoreTimer = 0f;
    [SerializeField] private float scoreInterval = 1f; // seconds per score increase

    private void Awake()
    {
        GameManager.Instance.IsInitialized = true;

        score = 0;
        _scoreText.text = score.ToString();
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        scoreTimer += Time.deltaTime;
        if (scoreTimer >= scoreInterval)
        {
            UpdateScore();
            scoreTimer = 0f;
        }
    }

    public void UpdateScore()
    {
        score++;
        _scoreText.text = score.ToString();
    }

    public void GameEnded()
    {
        isGameOver = true;
        GameManager.Instance.CurrentScore = score;
        StartCoroutine(GameOver());
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.GoToMainMenu();
    }
}
