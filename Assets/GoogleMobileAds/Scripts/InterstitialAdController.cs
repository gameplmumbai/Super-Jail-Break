using GoogleMobileAds.Api;
using GoogleMobileAds.Samples;
using System;
using UnityEngine;

public class InterstitialAdController
{
  //public static InterstitialAdController Instance { get; private set; }
  public static bool IsInterstitialAdAvailable { get { return ((_interstitialAd != null) && _interstitialAd.CanShowAd()); } }

  public static InterstitialAd _interstitialAd;



  /// <summary>
  /// Loads the ad.
  /// </summary>
  public static void LoadAd()
  {
    // Clean up the old ad before loading a new one.
    if (_interstitialAd != null)
    {
      DestroyAd();
    }

    Debug.Log("Loading interstitial ad.");

    // Create our request used to load the ad.
    var adRequest = new AdRequest();

    // Send the request to load the ad.
    InterstitialAd.Load(AdsController.Instance.InterstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
    {
      // If the operation failed with a reason.
      if (error != null)
      {
        Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
        return;
      }
      // If the operation failed for unknown reasons.
      // This is an unexpected error, please report this bug if it happens.
      if (ad == null)
      {
        Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
        return;
      }

      // The operation completed successfully.
      Debug.LogWarning("Interstitial ad loaded with response : " + ad.GetResponseInfo());
      _interstitialAd = ad;
      // Register to ad events to extend functionality.
      RegisterEventHandlers(ad);
      // Inform the UI that the ad is ready.
      //AdLoadedStatus?.SetActive(true);
    });
  }
  /// <summary>
  /// Shows the ad.
  /// </summary>
  public static void ShowAd()
  {
    if (_interstitialAd != null && _interstitialAd.CanShowAd())
    {
      Debug.Log("Showing interstitial ad.");
      _interstitialAd.Show();
    }
    else
    {
      Debug.LogWarning("Interstitial ad is not ready yet.");
    }

  }
  public static void ShowAd(Action<bool> adShownOnScreen)
  {
    if (_interstitialAd != null && _interstitialAd.CanShowAd())
    {
      Debug.Log("Showing interstitial ad.");
      _interstitialAd.Show();
      if (adShownOnScreen != null)
        adShownOnScreen(true);
    }
    else
    {
      Debug.LogWarning("Interstitial ad is not ready yet.");
      if (adShownOnScreen != null)
        adShownOnScreen(false);
    }

    // Inform the UI that the ad is not ready.
    //AdLoadedStatus?.SetActive(false);
  }

  /// <summary>
  /// Destroys the ad.
  /// </summary>
  public static void DestroyAd()
  {
    if (_interstitialAd != null)
    {
      Debug.Log("Destroying interstitial ad.");
      _interstitialAd.Destroy();
      _interstitialAd = null;
    }

    // Inform the UI that the ad is not ready.
    //AdLoadedStatus?.SetActive(false);
  }

  /// <summary>
  /// Logs the ResponseInfo.
  /// </summary>
  public static void LogResponseInfo()
  {
    if (_interstitialAd != null)
    {
      var responseInfo = _interstitialAd.GetResponseInfo();
      UnityEngine.Debug.Log(responseInfo);
    }
  }

  static void RegisterEventHandlers(InterstitialAd ad)
  {
    // Raised when the ad is estimated to have earned money.
    ad.OnAdPaid += (AdValue adValue) =>
    {
      Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    ad.OnAdImpressionRecorded += () =>
    {
      Debug.Log("Interstitial ad recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    ad.OnAdClicked += () =>
    {
      Debug.Log("Interstitial ad was clicked.");
    };
    // Raised when an ad opened full screen content.
    ad.OnAdFullScreenContentOpened += () =>
    {
      Debug.Log("Interstitial ad full screen content opened.");
    };
    // Raised when the ad closed full screen content.
    ad.OnAdFullScreenContentClosed += () =>
    {
      Debug.Log("Interstitial ad full screen content closed.");
      LoadAd();
    };
    // Raised when the ad failed to open full screen content.
    ad.OnAdFullScreenContentFailed += (AdError error) =>
    {
      Debug.LogError("Interstitial ad failed to open full screen content with error : "
                + error);
      //LoadAd();
    };
  }
}
