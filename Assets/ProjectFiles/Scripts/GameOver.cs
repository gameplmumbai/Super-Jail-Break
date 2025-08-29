using GamesLoki.GoogleMobileAds;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{


  public void StartGame()
  {
    SceneManager.LoadScene("Game");
    AdMobManager.Instance.ShowInterstitialAd();
  }

  public void GoToMenu()
  {
    SceneManager.LoadScene("Menu");
    AdMobManager.Instance.ShowInterstitialAd();
  }
}
