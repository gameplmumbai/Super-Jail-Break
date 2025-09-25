using GoogleMobileAds.Api;
using GoogleMobileAds.Samples;
using System;
using UnityEngine;


public class RewardedAdController
{
  public static bool IsRewardVideoAvailable { get { return ((_rewardedAd != null) && _rewardedAd.CanShowAd()); } }
  public static RewardedAd _rewardedAd;

  /// <summary>
  /// Loads the ad.
  /// </summary>
  public static void LoadAd()
  {

    // Clean up the old ad before loading a new one.
    if (_rewardedAd != null)
    {
      DestroyAd();
      Debug.Log($"_rewardedAd destroyed by LoadAD");
    }

    Debug.Log("Loading rewarded ad.");

    // Create our request used to load the ad.
    var adRequest = new AdRequest();

    // Send the request to load the ad.
    RewardedAd.Load(AdsController.Instance.RewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
    {
      // If the operation failed with a reason.
      if (error != null)
      {
        Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
        return;
      }
      // If the operation failed for unknown reasons.
      // This is an unexpected error, please report this bug if it happens.
      if (ad == null)
      {
        Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
        return;
      }

      // The operation completed successfully.
      Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
      _rewardedAd = ad;
      // Register to ad events to extend functionality.
      RegisterEventHandlers(ad);
    });
  }

  /// <summary>
  /// Shows the ad.
  /// </summary>
  public static void ShowAd()
  {
    if (_rewardedAd != null && _rewardedAd.CanShowAd())
    {
      Debug.Log("Showing rewarded ad.");
      _rewardedAd.Show((Reward reward) =>
      {
        Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                        reward.Amount,
                                        reward.Type));
      });
    }
    else
    {
      Debug.LogWarning("Rewarded ad is not ready yet.");
    }
  }

  public static void ShowAd(Action<bool> adShownOnScreen)
  {
    if (_rewardedAd != null && _rewardedAd.CanShowAd())
    {
      Debug.Log("Showing rewarded ad.");
      _rewardedAd.Show((Reward reward) =>
      {
        Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                        reward.Amount,
                                        reward.Type));
        if (adShownOnScreen != null)
          adShownOnScreen(true);
      });
    }
    else
    {
      Debug.LogWarning("Rewarded ad is not ready yet.");
      if (adShownOnScreen != null)
        adShownOnScreen(false);
    }
  }


  /// <summary>
  /// Destroys the ad.
  /// </summary>
  public static void DestroyAd()
  {
    if (_rewardedAd != null)
    {
      Debug.Log("Destroying rewarded ad.");
      _rewardedAd.Destroy();
      _rewardedAd = null;
    }
    // Inform the UI that the ad is not ready.
    //AdLoadedStatus?.SetActive(false);
  }

  /// <summary>
  /// Logs the ResponseInfo.
  /// </summary>
  public static void LogResponseInfo()
  {
    if (_rewardedAd != null)
    {
      var responseInfo = _rewardedAd.GetResponseInfo();
      UnityEngine.Debug.Log(responseInfo);
    }
  }

  static void RegisterEventHandlers(RewardedAd ad)
  {
    // Raised when the ad is estimated to have earned money.
    ad.OnAdPaid += (AdValue adValue) =>
    {

      Debug.Log(String.Format("Rewarded ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    ad.OnAdImpressionRecorded += () =>
    {
      Debug.Log("Rewarded ad recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    ad.OnAdClicked += () =>
    {
      Debug.Log("Rewarded ad was clicked.");
    };
    // Raised when the ad opened full screen content.
    ad.OnAdFullScreenContentOpened += () =>
    {
      Debug.Log("Rewarded ad full screen content opened.");
    };
    // Raised when the ad closed full screen content.
    ad.OnAdFullScreenContentClosed += () =>
    {
      Debug.Log("Rewarded ad full screen content closed.");
      LoadAd();

    };
    // Raised when the ad failed to open full screen content.
    ad.OnAdFullScreenContentFailed += (AdError error) =>
    {
      Debug.LogError("Rewarded ad failed to open full screen content with error : "
                + error);
      //LoadAd();
    };
  }
}
