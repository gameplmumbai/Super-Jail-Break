using GoogleMobileAds.Api;
using GoogleMobileAds.Samples;
using System;
using UnityEngine;
public class NativeOverlayAdController : MonoBehaviour
{

  /// <summary>
  /// Placeholder target for the native overlay ad.
  /// </summary>
  public static RectTransform AdPlacmentTarget;

  /// <summary>
  /// Define our native ad advanced options.
  /// </summary>
  public static NativeAdOptions Option = new NativeAdOptions
  {
    AdChoicesPlacement = AdChoicesPlacement.TopRightCorner,
    MediaAspectRatio = MediaAspectRatio.Any,
  };

  /// <summary>
  /// Define our native ad template style.
  /// </summary>
  public static NativeTemplateStyle Style = new NativeTemplateStyle
  {
    TemplateId = NativeTemplateId.Medium,
  };

  public static NativeOverlayAd _nativeOverlayAd;

  /// <summary>
  /// Loads the ad.
  /// </summary>
  public static void LoadAd()
  {

    // Clean up the old ad before loading a new one.
    if (_nativeOverlayAd != null)
    {
      DestroyAd();
    }

    Debug.Log("Loading native overlay ad.");

    // Create our request used to load the ad.
    var adRequest = new AdRequest();

    // Send the request to load the ad.
    NativeOverlayAd.Load(AdsController.Instance.NativeOverlayId, adRequest, Option,
        (NativeOverlayAd ad, LoadAdError error) =>
        {
          // If the operation failed with a reason.
          if (error != null)
          {
            Debug.LogError("Native Overlay ad failed to load an ad with error : " + error);
            return;
          }
          // If the operation failed for unknown reasons.
          // This is an unexpected error, please report this bug if it happens.
          if (ad == null)
          {
            Debug.LogError("Unexpected error: Native Overlay ad load event fired with " +
                  " null ad and null error.");
            return;
          }

          // The operation completed successfully.
          Debug.Log("Native Overlay ad loaded with response : " + ad.GetResponseInfo());
          _nativeOverlayAd = ad;

          // Register to ad events to extend functionality.
          RegisterEventHandlers(ad);

        });
  }

  static void RegisterEventHandlers(NativeOverlayAd ad)
  {
    // Raised when the ad is estimated to have earned money.
    ad.OnAdPaid += (AdValue adValue) =>
    {
      Debug.Log(String.Format("Native Overlay ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
    };
    // Raised when an impression is recorded for an ad.
    ad.OnAdImpressionRecorded += () =>
    {
      Debug.Log("Native Overlay ad recorded an impression.");
    };
    // Raised when a click is recorded for an ad.
    ad.OnAdClicked += () =>
    {
      Debug.Log("Native Overlay ad was clicked.");
    };
    // Raised when the ad opened full screen content.
    ad.OnAdFullScreenContentOpened += () =>
    {
      Debug.Log("Native Overlay ad full screen content opened.");
      LoadAd();
    };
    // Raised when the ad closed full screen content.
    ad.OnAdFullScreenContentClosed += () =>
    {
      Debug.Log("Native Overlay ad full screen content closed.");
      LoadAd();
    };
  }

  /// <summary>
  /// Shows the ad.
  /// </summary>
  public static void ShowAd()
  {
    if (_nativeOverlayAd != null)
    {
      Debug.Log("Showing Native Overlay ad.");
      _nativeOverlayAd.Show();
    }
  }

  /// <summary>
  /// Hides the ad.
  /// </summary>
  public static void HideAd()
  {
    if (_nativeOverlayAd != null)
    {
      Debug.Log("Hiding Native Overlay ad.");
      _nativeOverlayAd.Hide();
    }
  }

  /// <summary>
  /// Renders the ad.
  /// </summary>
  public static void RenderAd()
  {
    if (_nativeOverlayAd != null)
    {
      Debug.Log("Rendering Native Overlay ad.");

      // Renders a native overlay ad at the default size
      // and anchored to the bottom of the screne.
      _nativeOverlayAd.RenderTemplate(Style, AdPosition.Bottom);
    }
  }

  /// <summary>
  /// Destroys the ad.
  /// When you are finished with the ad, make sure to call the Destroy()
  /// method before dropping your reference to it.
  /// </summary>
  public static void DestroyAd()
  {
    if (_nativeOverlayAd != null)
    {
      Debug.Log("Destroying Native Overlay ad.");
      _nativeOverlayAd.Destroy();
      _nativeOverlayAd = null;
    }

  }

  /// <summary>
  /// Logs the ResponseInfo.
  /// </summary>
  public static void LogResponseInfo()
  {
    if (_nativeOverlayAd != null)
    {
      var responseInfo = _nativeOverlayAd.GetResponseInfo();
      if (responseInfo != null)
      {
        Debug.Log(responseInfo);
      }
    }
  }
}
