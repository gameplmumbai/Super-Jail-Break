using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Samples;
using System;
using UnityEngine;
public class AppOpenAdController
{
  // App open ads can be preloaded for up to 4 hours.
  static readonly TimeSpan TIMEOUT = TimeSpan.FromHours(4);
  static DateTime _expireTime;

  public static bool IsAppOpenAdAvailable { get { return (_appOpenAd != null && _appOpenAd.CanShowAd()); } }

  public static AppOpenAd _appOpenAd;
  public static void Awake()
  {
    // Use the AppStateEventNotifier to listen to application open/close events.
    // This is used to launch the loaded ad when we open the app.
    Debug.Log("AppOpenAd Awake");
    AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
  }
  public static void OnDestroy()
  {
    // Always unlisten to events when complete.
    try
    {
      AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    }
    catch (Exception)
    {
      Debug.Log("AppOpenAd OnDestroy");
    }

  }


  /// <summary>
  /// Loads the ad.
  /// </summary>
  public static void LoadAd()
  {

    // Clean up the old ad before loading a new one.
    if (_appOpenAd != null)
    {
      DestroyAd();
    }

    Debug.Log("Loading app open ad.");

    // Create our request used to load the ad.
    var adRequest = new AdRequest();

    // Send the request to load the ad.
    AppOpenAd.Load(AdsController.Instance.AppOpenId, adRequest, (AppOpenAd ad, LoadAdError error) =>
    {
      // If the operation failed with a reason.
      if (error != null)
      {
        Debug.LogError("App open ad failed to load an ad with error : "
                              + error);
        return;
      }

      // If the operation failed for unknown reasons.
      // This is an unexpected error, please report this bug if it happens.
      if (ad == null)
      {
        Debug.LogError("Unexpected error: App open ad load event fired with " +
                             " null ad and null error.");
        return;
      }

      // The operation completed successfully.
      Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());
      _appOpenAd = ad;

      // App open ads can be preloaded for up to 4 hours.
      _expireTime = DateTime.Now + TIMEOUT;

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
    // App open ads can be preloaded for up to 4 hours.
    if (_appOpenAd != null && _appOpenAd.CanShowAd() && DateTime.Now < _expireTime)
    {
      Debug.Log("Showing app open ad.");
      _appOpenAd.Show();
    }
    else
    {
      Debug.Log("App open ad is not ready yet.");
    }

    // Inform the UI that the ad is not ready.
    //AdLoadedStatus?.SetActive(false);
  }

  /// <summary>
  /// Destroys the ad.
  /// </summary>
  public static void DestroyAd()
  {
    if (_appOpenAd != null)
    {
      Debug.Log("Destroying app open ad.");
      _appOpenAd.Destroy();
      _appOpenAd = null;
    }

    // Inform the UI that the ad is not ready.
    //AdLoadedStatus?.SetActive(false);
  }

  /// <summary>
  /// Logs the ResponseInfo.
  /// </summary>
  public static void LogResponseInfo()
  {
    if (_appOpenAd != null)
    {
      var responseInfo = _appOpenAd.GetResponseInfo();
      UnityEngine.Debug.Log(responseInfo);
    }
  }

  private static void OnAppStateChanged(AppState state)
  {
    Debug.Log("App State changed to : " + state);

    // If the app is Foregrounded and the ad is available, show it.
    if (state == AppState.Foreground)
    {
      ShowAd();
    }
  }

  private static void RegisterEventHandlers(AppOpenAd ad)
  {
    // Raised when the ad is estimated to have earned money.
    ad.OnAdPaid += (AdValue adValue) =>
    {
      Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    ad.OnAdImpressionRecorded += () =>
    {
      Debug.Log("App open ad recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    ad.OnAdClicked += () =>
    {
      Debug.Log("App open ad was clicked.");
    };
    // Raised when an ad opened full screen content.
    ad.OnAdFullScreenContentOpened += () =>
    {
      Debug.Log("App open ad full screen content opened.");

      // Inform the UI that the ad is consumed and not ready.
      //AdLoadedStatus?.SetActive(false);
    };
    // Raised when the ad closed full screen content.
    ad.OnAdFullScreenContentClosed += () =>
    {
      Debug.Log("App open ad full screen content closed.");
      LoadAd();
      // It may be useful to load a new ad when the current one is complete.
    };
    // Raised when the ad failed to open full screen content.
    ad.OnAdFullScreenContentFailed += (AdError error) =>
    {
      Debug.LogError("App open ad failed to open full screen content with error : "
                            + error);
      LoadAd();
    };
  }
}
