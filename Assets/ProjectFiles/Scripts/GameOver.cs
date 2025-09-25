using GoogleMobileAds.Samples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{


  public void StartGame()
  {
    SceneManager.LoadScene("Game");
        AdsController.Instance.ShowInterstitialAd();
  }

  public void GoToMenu()
  {
    SceneManager.LoadScene("Menu");
        AdsController.Instance.ShowInterstitialAd();
  }
}
