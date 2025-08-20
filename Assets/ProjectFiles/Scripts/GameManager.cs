using BeautifulTransitions.Scripts.Transitions.Components.GameObject;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

  public static GameManager instance;
  public Text LevelText;
  [Header("UI")]
  public GameObject GameOverUI;
  public GameObject GameWinUI;
  public GameObject Tutorial;
  public GameObject RedAlert;
  [Header("Level")]
  public GameObject[] Levels;
  // Start is called before the first frame update
  int Level;
  int TotalPrisoner, PCount;

  bool isGameEnd;

  public void Awake()
  {
    if (instance == null)
      instance = this;
    else if (instance != this)
      Destroy(gameObject);
  }

  void Start()
  {
    RedAlert.SetActive(false);
    TotalPrisoner = 0;
    isGameEnd = false;
    GameOverUI.SetActive(false);
    GameWinUI.SetActive(false);
    Level = PlayerPrefs.GetInt("Level", 0);
    LevelText.text = "Level " + (Level + 1);
    Instantiate(Levels[Level]);
    if (Level == 0)
    {
      Tutorial.SetActive(true);
      StartCoroutine(EndTutorial());
    }
    TransitionScale[] transitionSettings = FindObjectsByType<TransitionScale>(FindObjectsSortMode.None);
    foreach (var item in transitionSettings)
    {
      item.TransitionInConfig.TransitionType = BeautifulTransitions.Scripts.Transitions.TransitionHelper.TweenType.linear;
      item.TransitionOutConfig.TransitionType = BeautifulTransitions.Scripts.Transitions.TransitionHelper.TweenType.linear;
    }

  }

  void Update()
  {
    if (PathCreator.instance.Ready)
    {
      if (PCount == 3)
      {
        LevelCompleted();
      }
    }

  }

  public void LoadMenu()
  {

    SceneManager.LoadScene("Menu");
  }

  public void RetryLevel()
  {
    SceneManager.LoadScene("Game");
  }

  public void GameOver()
  {
    isGameEnd = true;
    RedAlert.SetActive(true);
    StartCoroutine(GameOverSceen());

    // GameOverUI.SetActive(true);
  }

  public void LevelCompleted()
  {
    if (!isGameEnd)
    {

      Level++;
      PlayerPrefs.SetInt("Level", Level);
      //GameWinUI.SetActive(true);
      SceneManager.LoadScene("GameWin");
      isGameEnd = true;
      AdsManager.Instance.ShowInterstitialAd();
    }
  }

  IEnumerator EndTutorial()
  {
    yield return new WaitForSeconds(4.0f);
    Tutorial.SetActive(false);
  }

  public void UpdateScore()
  {
    PCount++;

  }

  public void TotalPrisonerCount(int count)
  {
    TotalPrisoner = count;
    Debug.Log(TotalPrisoner + "........................");
  }


  IEnumerator GameOverSceen()
  {
    yield return new WaitForSeconds(2.0f);
    SceneManager.LoadScene("GameOver");
  }


}
