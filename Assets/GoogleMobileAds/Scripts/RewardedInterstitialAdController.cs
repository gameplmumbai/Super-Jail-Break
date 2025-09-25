using GoogleMobileAds.Api;
using GoogleMobileAds.Samples;
using System;
using UnityEngine;

public class RewardedInterstitialAdController
{
  public static bool IsRewardedInterstitialAdAvailable { get { return (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd()); } }

  public static RewardedInterstitialAd _rewardedInterstitialAd;

  /// <summary>
  /// Loads the ad.
  /// </summary>
  public static void LoadAd()
  {

    // Clean up the old ad before loading a new one.
    if (_rewardedInterstitialAd != null)
    {
      DestroyAd();
    }

    Debug.Log("Loading rewarded interstitial ad.");

    // Create our request used to load the ad.
    var adRequest = new AdRequest();

    // Send the request to load the ad.
    RewardedInterstitialAd.Load(AdsController.Instance.RewardedInterstitialId, adRequest,
        (RewardedInterstitialAd ad, LoadAdError error) =>
        {
          // If the operation failed with a reason.
          if (error != null)
          {
            Debug.LogError("Rewarded interstitial ad failed to load an ad with error : "
                                  + error);
            return;
          }
          // If the operation failed for unknown reasons.
          // This is an unexpexted error, please report this bug if it happens.
          if (ad == null)
          {
            Debug.LogError("Unexpected error: Rewarded interstitial load event fired with null ad and null error.");
            return;
          }

          // The operation completed successfully.
          Debug.Log("Rewarded interstitial ad loaded with response : "
                    + ad.GetResponseInfo());
          _rewardedInterstitialAd = ad;

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
    if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
    {
      _rewardedInterstitialAd.Show((Reward reward) =>
      {
        Debug.Log("Rewarded interstitial ad rewarded : " + reward.Amount);
      });
    }
    else
    {
      Debug.LogWarning("Rewarded interstitial ad is not ready yet.");
    }

  }


  /// <summary>
  /// Shows the ad.
  /// </summary>
  public static void ShowAd(Action<bool> adShownOnScreen)
  {
    if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
    {
      _rewardedInterstitialAd.Show((Reward reward) =>
      {
        Debug.Log("Rewarded interstitial ad rewarded : " + reward.Amount);
      });
      if (adShownOnScreen != null)
        adShownOnScreen(true);
    }
    else
    {
      Debug.LogWarning("Rewarded interstitial ad is not ready yet.");
      if (adShownOnScreen != null)
        adShownOnScreen(false);
    }
  }



  /// <summary>
  /// Destroys the ad.
  /// </summary>
  public static void DestroyAd()
  {
    if (_rewardedInterstitialAd != null)
    {
      Debug.Log("Destroying rewarded interstitial ad.");
      _rewardedInterstitialAd.Destroy();
      _rewardedInterstitialAd = null;
    }

    // Inform the UI that the ad is not ready.
    //AdLoadedStatus?.SetActive(false);
  }

  /// <summary>
  /// Logs the ResponseInfo.
  /// </summary>
  public static void LogResponseInfo()
  {
    if (_rewardedInterstitialAd != null)
    {
      var responseInfo = _rewardedInterstitialAd.GetResponseInfo();
      UnityEngine.Debug.Log(responseInfo);
    }
  }

  protected static void RegisterEventHandlers(RewardedInterstitialAd ad)
  {
    // Raised when the ad is estimated to have earned money.
    ad.OnAdPaid += (AdValue adValue) =>
    {
      Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    ad.OnAdImpressionRecorded += () =>
    {
      Debug.Log("Rewarded interstitial ad recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    ad.OnAdClicked += () =>
    {
      Debug.Log("Rewarded interstitial ad was clicked.");
    };
    // Raised when an ad opened full screen content.
    ad.OnAdFullScreenContentOpened += () =>
    {
      Debug.Log("Rewarded interstitial ad full screen content opened.");
    };
    // Raised when the ad closed full screen content.
    ad.OnAdFullScreenContentClosed += () =>
    {
      Debug.Log("Rewarded interstitial ad full screen content closed.");
      LoadAd();
    };
    // Raised when the ad failed to open full screen content.
    ad.OnAdFullScreenContentFailed += (AdError error) =>
    {
      Debug.LogError("Rewarded interstitial ad failed to open full screen content" +
                           " with error : " + error);
      //LoadAd();
    };
  }
}
