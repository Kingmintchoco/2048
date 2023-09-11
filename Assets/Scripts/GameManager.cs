using TMPro;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TileBoard borad;
    public CanvasGroup gameOver;

    private int score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hiscoreText;

    private void Start(){
        NewGame();
    }

    public void NewGame(){
        SetScore(0);
        hiscoreText.text = LoadHiScore().ToString();
        
        gameOver.alpha = 0f;
        gameOver.interactable = false;

        borad.ClearBoard();
        borad.CreateTile();
        borad.CreateTile();
        borad.enabled = true;
    }

    public void GameOver(){
        borad.enabled = false;
        gameOver.interactable = true;

        StartCoroutine(Fade(gameOver, 1f, 1f));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay){
        yield return new WaitForSeconds(delay);

        float elaspsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while(elaspsed < duration){
            canvasGroup.alpha = Mathf.Lerp(from, to, elaspsed / duration);
            elaspsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void IncreaseScore(int points){
        SetScore(score + points);
    }

    private void SetScore(int score){
        this.score = score;
        scoreText.text = score.ToString();
        SaveHiScore();
    }

    private void SaveHiScore(){
        int hiscore = LoadHiScore();

        if(score > hiscore){
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    private int LoadHiScore(){
        return PlayerPrefs.GetInt("hiscore", 0);
    }
}
