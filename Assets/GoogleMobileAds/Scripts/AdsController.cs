using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Samples;
using GoogleMobileAds.Ump.Api;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GoogleMobileAds.Samples
{
  /// <summary>
  /// Demonstrates how to use the Google Mobile Ads Unity plugin.
  /// </summary>
  [AddComponentMenu("GoogleMobileAds/Samples/GoogleMobileAdsController")]
  public class AdsController : MonoBehaviour
  {
    #region Constants
    public const string VAR_REMOVEADS = "removead";
    #endregion

    #region Vars
    // The Google Mobile Ads Unity plugin needs to be run only once.
    private static bool? _isInitialized;

    // Helper class that implements consent using the
    // Google User Messaging Platform (UMP) Unity plugin.
    [SerializeField, Tooltip("Controller for the Google User Messaging Platform (UMP) Unity plugin.")]
    private AdsConsentController _consentController;


    [SerializeField]
    bool EnableAdMobModule;


    [Space(15)]
    [Header("AdMob Unit IDs")]
    public string AndroidAppId;
    public enum BannerAdSize { Banner, SmartBanner, MediumRectangle, IABBanner, Leaderboard }

    [Space(15)]
    [Header("Ads Recurring On Load Fail Observer Moniter Tick")]
    [Range(5, 300)]
    public float Recurring_PreLoad_Interval_Time;

    [Space(15)]
    [Header("Banner Ad Settings")]
    public bool Opt_BannerAd;
    public string BannerId;
    public AdPosition Banner_AdPosition;
    public BannerAdSize Banner_AdSize;

    [Space(15)]
    [Header("Interstitial Ad Settings")]
    public bool Opt_InterstitialAd;
    public string InterstitialId;
    public bool Exclude_InterstitialAd_From_RemoveAd;


    [Space(15)]
    [Header("Rewarded Ad Settings")]
    public bool Opt_RewardedAd;
    public string RewardedId;
    public bool Exclude_RewardedAd_From_RemoveAd;


    [Space(15)]
    [Header("Rewarded Interstitial Ad Settings")]
    public bool Opt_RewardedInterstitialAd;
    public string RewardedInterstitialId;
    public bool Exclude_RewardedInterstitialAd_From_RemoveAd;

    [Space(15)]
    [Header("Native Over Ad Settings")]
    public bool Opt_NativeOverlayAd;
    public string NativeOverlayId;

    [Space(15)]
    [Header("App Open Ad Settings")]
    public bool Opt_AppOpenAd;
    public string AppOpenId;


    public static AdsController Instance { get; private set; }

    public static bool IsConnectedToInternet { get { return AppManager.IsInternetAvailable(); } }
    public void SetTestIds()
    {
      AndroidAppId = "ca-app-pub-3940256099942544~3347511713";
      BannerId = "ca-app-pub-3940256099942544/6300978111";
      InterstitialId = "ca-app-pub-3940256099942544/1033173712";
      RewardedId = "ca-app-pub-3940256099942544/5224354917";
      RewardedInterstitialId = "ca-app-pub-3940256099942544/5354046379";
      NativeOverlayId = "ca-app-pub-3940256099942544/2247696110";
      AppOpenId = "ca-app-pub-3940256099942544/9257395921";
    }
    #endregion

    #region MonoBehaviour

    // Always use test ads.
    // https://developers.google.com/admob/unity/test-ads
    internal static List<string> TestDeviceIds = new List<string>()
            {
                AdRequest.TestDeviceSimulator,
    #if UNITY_IPHONE
                "96e23e80653bb28980d3f40beb58915c",
    #elif UNITY_ANDROID
                "702815ACFC14FF222DA1DC767672A573"
    #endif
            };



    private void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }
      if (!EnableAdMobModule) { return; }
      AppOpenAdController.Awake();
    }

    /// <summary>
    /// Demonstrates how to configure Google Mobile Ads Unity plugin.
    /// </summary>
    private void Start()
    {
      if (!EnableAdMobModule) { return; }
      if (!IsConnectedToInternet) { return; }
      // On Android, Unity is paused when displaying interstitial or rewarded video.
      // This setting makes iOS behave consistently with Android.
      MobileAds.SetiOSAppPauseOnBackground(true);

      // Configure your RequestConfiguration with Child Directed Treatment
      // and the Test Device Ids.
      MobileAds.SetRequestConfiguration(new RequestConfiguration
      {
        TestDeviceIds = TestDeviceIds
      });

      // If we can request ads, we should initialize the Google Mobile Ads Unity plugin.
      if (_consentController.CanRequestAds)
      {
        InitializeGoogleMobileAds();
      }

      // Ensures that privacy and consent information is up to date.
      InitializeGoogleMobileAdsConsent();
    }

    public void RemoveAdsEntitled()
    {
      Time.timeScale = 1;
      BannerViewController.DestroyAd();
      InterstitialAdController.DestroyAd();
      RewardedAdController.DestroyAd();
      RewardedInterstitialAdController.DestroyAd();
      NativeOverlayAdController.DestroyAd();
      AppOpenAdController.DestroyAd();
      foreach (var item in FindObjectsByType<AdObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
      {
        Destroy(item.gameObject);
      }
    }

    /// <summary>
    /// Ensures that privacy and consent information is up to date.
    /// </summary>
    private void InitializeGoogleMobileAdsConsent()
    {
      Debug.Log("Google Mobile Ads gathering consent.");

      _consentController.GatherConsent((string error) =>
      {
        if (error != null)
        {
          Debug.LogError("Failed to gather consent with error: " +
                    error);
        }
        else
        {
          Debug.Log("Google Mobile Ads consent updated: "
                    + ConsentInformation.ConsentStatus);
        }

        if (_consentController.CanRequestAds)
        {
          InitializeGoogleMobileAds();
        }
      });
    }

    /// <summary>
    /// Initializes the Google Mobile Ads Unity plugin.
    /// </summary>
    private void InitializeGoogleMobileAds()
    {
      // The Google Mobile Ads Unity plugin needs to be run only once and before loading any ads.
      if (_isInitialized.HasValue)
      {
        return;
      }

      _isInitialized = false;

      // Initialize the Google Mobile Ads Unity plugin.
      Debug.Log("Google Mobile Ads Initializing.");
      MobileAds.RaiseAdEventsOnUnityMainThread = true;
      // [START initialize_sdk]
      MobileAds.Initialize((InitializationStatus initstatus) =>
      {
        if (initstatus == null)
        {
          Debug.LogError("Google Mobile Ads initialization failed.");
          _isInitialized = null;
          return;
        }
        // [START_EXCLUDE silent]
        // If you use mediation, you can check the status of each adapter.
        var adapterStatusMap = initstatus.getAdapterStatusMap();
        if (adapterStatusMap != null)
        {
          foreach (var item in adapterStatusMap)
          {
            Debug.Log(string.Format("Adapter {0} is {1}",
                      item.Key,
                      item.Value.InitializationState));
          }
        }
        // [END_EXCLUDE]

        Debug.Log("Google Mobile Ads initialization complete.");
        _isInitialized = true;
        Ad_PreloadFormer();
        // Google Mobile Ads events are raised off the Unity Main thread. If you need to
        // access UnityEngine objects after initialization,
        // use MobileAdsEventExecutor.ExecuteInUpdate(). For more information, see:
        // https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread
      });
      // [END initialize_sdk]
    }

    void Ad_PreloadFormer()
    {
      if (Opt_BannerAd)
      {
        BannerAd_PreLoad_ADs();
        CancelInvoke(nameof(BannerAd_PreLoad_ADs));
        InvokeRepeating(nameof(BannerAd_PreLoad_ADs), 1, Recurring_PreLoad_Interval_Time);
      }
      if (Opt_InterstitialAd)
      {
        InterstitialAd_PreLoad_ADs();
        CancelInvoke(nameof(InterstitialAd_PreLoad_ADs));
        InvokeRepeating(nameof(InterstitialAd_PreLoad_ADs), 1, Recurring_PreLoad_Interval_Time);
      }
      if (Opt_RewardedAd)
      {
        RewardedAd_PreLoad_ADs();
        CancelInvoke(nameof(RewardedAd_PreLoad_ADs));
        InvokeRepeating(nameof(RewardedAd_PreLoad_ADs), 1, Recurring_PreLoad_Interval_Time);
      }
      if (Opt_RewardedInterstitialAd)
      {
        RewardedInterstitialAd_PreLoad_ADs();
        CancelInvoke(nameof(RewardedInterstitialAd_PreLoad_ADs));
        InvokeRepeating(nameof(RewardedInterstitialAd_PreLoad_ADs), 1, Recurring_PreLoad_Interval_Time);
      }
      if (Opt_NativeOverlayAd)
      {
        NativeOverlayAd_PreLoad_ADs();
        CancelInvoke(nameof(NativeOverlayAd_PreLoad_ADs));
        InvokeRepeating(nameof(NativeOverlayAd_PreLoad_ADs), 1, Recurring_PreLoad_Interval_Time);
      }
      if (Opt_AppOpenAd)
      {
        AppOpenAd_PreLoad_ADs();
        CancelInvoke(nameof(AppOpenAd_PreLoad_ADs));
        InvokeRepeating(nameof(AppOpenAd_PreLoad_ADs), 1, Recurring_PreLoad_Interval_Time);
      }

    }

    private void OnDestroy()
    {
      try
      {
        AppOpenAdController.OnDestroy();
      }
      catch
      {
      }
    }

    /// <summary>
    /// Opens the AdInspector.
    /// </summary>
    public void OpenAdInspector()
    {
      Debug.Log("Opening ad Inspector.");
      MobileAds.OpenAdInspector((AdInspectorError error) =>
      {
        // If the operation failed, an error is returned.
        if (error != null)
        {
          Debug.Log("Ad Inspector failed to open with error: " + error);
          return;
        }

        Debug.Log("Ad Inspector opened successfully.");
      });
    }

    /// <summary>
    /// Opens the privacy options form for the user.
    /// </summary>
    /// <remarks>
    /// Your app needs to allow the user to change their consent status at any time.
    /// </remarks>
    public void OpenPrivacyOptions()
    {
      _consentController.ShowPrivacyOptionsForm((string error) =>
      {
        if (error != null)
        {
          Debug.LogError("Failed to show consent privacy form with error: " +
                    error);
        }
        else
        {
          Debug.Log("Privacy form opened successfully.");
        }
      });
    }
    #endregion

    #region LoadAds

    void BannerAd_PreLoad_ADs()
    {
      if (  Opt_BannerAd && BannerViewController._bannerView == null)
      {
        BannerViewController.LoadAd();
      }
    }
    void InterstitialAd_PreLoad_ADs()
    {
      if ((Exclude_InterstitialAd_From_RemoveAd ) && Opt_InterstitialAd)
      {
        if (InterstitialAdController._interstitialAd == null || !InterstitialAdController._interstitialAd.CanShowAd())
        {
          InterstitialAdController.LoadAd();
        }
      }
    }

    void RewardedAd_PreLoad_ADs()
    {
      if ((Exclude_RewardedAd_From_RemoveAd ) && Opt_RewardedAd)
      {
        if (RewardedAdController._rewardedAd == null || !RewardedAdController._rewardedAd.CanShowAd())
        {
          RewardedAdController.LoadAd();
        }
      }
    }
    void RewardedInterstitialAd_PreLoad_ADs()
    {
      if ((Exclude_RewardedInterstitialAd_From_RemoveAd ) && Opt_RewardedInterstitialAd)
      {
        if (RewardedInterstitialAdController._rewardedInterstitialAd == null || !RewardedInterstitialAdController._rewardedInterstitialAd.CanShowAd())
        {
          RewardedInterstitialAdController.LoadAd();
        }
      }
    }

    void NativeOverlayAd_PreLoad_ADs()
    {
      if ( Opt_NativeOverlayAd)
      {
        if (NativeOverlayAdController._nativeOverlayAd == null)
        {
          NativeOverlayAdController.LoadAd();
        }
      }
    }

    void AppOpenAd_PreLoad_ADs()
    {
      if ( Opt_AppOpenAd)
      {
        if (AppOpenAdController._appOpenAd == null || !AppOpenAdController._appOpenAd.CanShowAd())
        {
          AppOpenAdController.LoadAd();
        }
      }
    }


    #endregion

    #region Banner Section

    public void ShowBannerAd()
    {
      if (!EnableAdMobModule) { return; }
      if (!Opt_BannerAd) { return; }
      BannerViewController.ShowAd();
    }

    public void HideBannerAd()
    {
      if (!Opt_BannerAd) { return; }
      BannerViewController.ShowAd();
    }

    #endregion

    #region Interstitial Section
    public void ShowInterstitialAd()
    {
      if (!EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }
      Debug.Log("ShowInterstitialAd");
      if (!Opt_InterstitialAd)
      {
        Debug.Log($"Not opted for InterstitialAds");
        return;
      }
      if (InterstitialAdController._interstitialAd == null)
      {
        InterstitialAdController.LoadAd();
        Debug.Log($"InterstitialAd is null");
        return;
      }
      if (!InterstitialAdController._interstitialAd.CanShowAd())
      {
        Debug.Log($"InterstitialAd is not ready to show this time retry again");
        return;
      }
      InterstitialAdController.ShowAd();
    }

    public void ShowInterstitialAd(Action<bool> adDisplayedonScreen_callback = null)
    {
      if (!EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }

      Debug.Log("ShowInterstitialAd");
      if (!Opt_InterstitialAd)
      {
        Debug.Log($"Not opted for InterstitialAds");
        return;
      }
      if (InterstitialAdController._interstitialAd == null)
      {
        InterstitialAdController.LoadAd();
        Debug.Log($"InterstitialAd is null");
        return;
      }
      if (!InterstitialAdController._interstitialAd.CanShowAd())
      {
        Debug.Log($"InterstitialAd is not ready to show this time retry again");
        return;
      }
      InterstitialAdController.ShowAd(adDisplayedonScreen_callback);
    }


    #endregion

    #region Rewarded Section

    public bool IsRewardedAdVideoAvailable { get { return RewardedAdController.IsRewardVideoAvailable; } }

    public void ShowRewardedAd()
    {
      //if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      if (!EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }

      Debug.Log("ShowRewardedAd");
      if (!Opt_RewardedAd)
      {
        Debug.Log($"Not opted for RewardedAds");
        return;
      }
      if (RewardedAdController._rewardedAd == null)
      {
        RewardedAdController.LoadAd();
        Debug.Log($"RewardedAd is null");
        return;
      }
      if (!RewardedAdController._rewardedAd.CanShowAd())
      {
        Debug.Log($"RewardedAd is not ready to show this time retry again");
        return;
      }
      RewardedAdController.ShowAd();
    }

    public void ShowRewardedAd(Action<bool> adDisplayedonScreen_callback = null)
    {
      //if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      if ( !EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }

      Debug.Log("ShowRewardedAd");
      if (!Opt_RewardedAd)
      {
        Debug.Log($"Not opted for RewardedAds");
        return;
      }
      if (RewardedAdController._rewardedAd == null)
      {
        RewardedAdController.LoadAd();
        Debug.Log($"RewardedAd is null");
        return;
      }
      if (!RewardedAdController._rewardedAd.CanShowAd())
      {
        Debug.Log($"RewardedAd is not ready to show this time retry again");
        return;
      }
      RewardedAdController.ShowAd(adDisplayedonScreen_callback);
    }

    #endregion

    #region RewardedInterstitial Section

    public bool IsRewardedInterstitialAdVideoAvailable { get { return RewardedInterstitialAdController.IsRewardedInterstitialAdAvailable; } }
    public void ShowRewardedInterstitialAd()
    {
      //if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      if (!EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }

      Debug.Log("ShowRewardedInterstitialAd");
      if (!Opt_RewardedInterstitialAd)
      {
        Debug.Log($"Not opted for RewardedInterstitialAds");
        return;
      }
      if (RewardedInterstitialAdController._rewardedInterstitialAd == null)
      {
        RewardedInterstitialAdController.LoadAd();
        Debug.Log($"RewardedInterstitialAd is null");
        return;
      }
      if (!RewardedInterstitialAdController._rewardedInterstitialAd.CanShowAd())
      {
        Debug.Log($"RewardedInterstitialAd is not ready to show this time retry again");
        return;
      }
      RewardedInterstitialAdController.ShowAd();
    }
    public void ShowRewardedInterstitialAd(Action<bool> adDisplayedonScreen_callback = null)
    {
      //if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      if ( !EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }

      Debug.Log("ShowRewardedInterstitialAd");
      if (!Opt_RewardedInterstitialAd)
      {
        Debug.Log($"Not opted for RewardedInterstitialAds");
        return;
      }
      if (RewardedInterstitialAdController._rewardedInterstitialAd == null)
      {
        RewardedInterstitialAdController.LoadAd();
        Debug.Log($"RewardedInterstitialAd is null");
        return;
      }
      if (!RewardedInterstitialAdController._rewardedInterstitialAd.CanShowAd())
      {
        Debug.Log($"RewardedInterstitialAd is not ready to show this time retry again");
        return;
      }
      RewardedInterstitialAdController.ShowAd(adDisplayedonScreen_callback);
    }
    #endregion

    #region AdMob NativeAdController Section

    public void LoadNativeOverlayAd()
    {
      //if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      if (!EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }


      Debug.Log("ShowNativeOverlayAd");
      if (!Opt_NativeOverlayAd)
      {
        Debug.Log($"Not opted for NativeOverlayAds");
        return;
      }
      if (NativeOverlayAdController._nativeOverlayAd == null)
      {
        NativeOverlayAdController.LoadAd();
        Debug.Log($"NativeOverlayAd is null");
        return;
      }

      NativeOverlayAdController.LoadAd();
    }

    #endregion

    #region App Open Section

    public void LoadAppOpenAd()
    {
      //if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      if ( !EnableAdMobModule) { return; }
      if ((_isInitialized == null || _isInitialized == false) || !IsConnectedToInternet) { return; }

      Debug.Log("ShowAppOpenAd");
      if (!Opt_AppOpenAd)
      {
        Debug.Log($"Not opted for AppOpenAds");
        return;
      }
      if (AppOpenAdController._appOpenAd == null)
      {
        AppOpenAdController.LoadAd();
        Debug.Log($"_appOpenAd == null");
        return;
      }
      if (!AppOpenAdController._appOpenAd.CanShowAd())
      {
        Debug.Log($"_appOpenAd is not ready to show this time retry again");
        return;
      }

      AppOpenAdController.LoadAd();
    }


    #endregion

  }

}

#if UNITY_EDITOR
[CustomEditor(typeof(AdsController))]
public class MyScriptEditor : Editor
{
  public override void OnInspectorGUI()
  {
    AdsController myScript = (AdsController)target;

    SerializedProperty property = serializedObject.GetIterator();
    bool expanded = property.NextVisible(true);
    if (GUILayout.Button("Set Test Ids"))
    {
      myScript.SetTestIds();
    }
    while (expanded)
    {

      if (property.name != "m_Script")
      {
        if (property.name == "BannerId")
        {
          if (myScript.Opt_BannerAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BannerId"));
        }
        else if (property.name == "Banner_AdPosition")
        {
          if (myScript.Opt_BannerAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Banner_AdPosition"));
        }
        else if (property.name == "Banner_AdSize")
        {
          if (myScript.Opt_BannerAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Banner_AdSize"));
        }
        else if (property.name == "InterstitialId")
        {
          if (myScript.Opt_InterstitialAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InterstitialId"));
        }
        else if (property.name == "Exclude_InterstitialAd_From_RemoveAd")
        {
          if (myScript.Opt_InterstitialAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Exclude_InterstitialAd_From_RemoveAd"));
        }

        else if (property.name == "RewardedId")
        {
          if (myScript.Opt_RewardedAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RewardedId"));
        }
        else if (property.name == "Exclude_RewardedAd_From_RemoveAd")
        {
          if (myScript.Opt_RewardedAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Exclude_RewardedAd_From_RemoveAd"));
        }

        else if (property.name == "RewardedInterstitialId")
        {
          if (myScript.Opt_RewardedInterstitialAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RewardedInterstitialId"));
        }
        else if (property.name == "Exclude_RewardedInterstitialAd_From_RemoveAd")
        {
          if (myScript.Opt_RewardedInterstitialAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Exclude_RewardedInterstitialAd_From_RemoveAd"));
        }

        else if (property.name == "NativeOverlayId")
        {
          if (myScript.Opt_NativeOverlayAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NativeOverlayId"));
        }

        else if (property.name == "AppOpenId")
        {
          if (myScript.Opt_AppOpenAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AppOpenId"));
        }

        else
        {
          EditorGUILayout.PropertyField(property, true);
        }
      }

      expanded = property.NextVisible(false);
    }


    serializedObject.ApplyModifiedProperties();
  }

}
#endif


