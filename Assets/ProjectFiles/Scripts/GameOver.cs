using GamesLoki.GoogleMobileAds;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{


  public void StartGame()
  {
    AdMobManager.Instance.ShowInterstitialAd();
    SceneManager.LoadScene("Game");
  }

  public void GoToMenu()
  {
    SceneManager.LoadScene("Menu");
  }
}
