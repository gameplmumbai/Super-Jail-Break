using GamesLoki.GoogleMobileAds;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamesLoki.GoogleMobileAds
{

  public class AdMobManager : MonoBehaviour
  {
    #region Vars
    [SerializeField]
    public bool ForceInternetRequired;
    [SerializeField]
    bool AsModuleEnabled;

    [SerializeField]
    bool IsGoogle_MobileAds_Initilised;


    [Space(15)]
    [Header("AdMob Unit IDs")]
    public string AndroidAppId;
    public string BannerId;
    public string InterstitialId;
    public string RewardedId;
    public string RewardedInterstitialId;
    public string NativeOverlayId;
    public string AppOpenId;


    public enum BannerAdSize { Banner, MediumRectangle, IABBanner, Leaderboard }

    [Space(15)]
    [Header("Opt Section")]
    public bool Opt_BannerAd;
    public AdPosition Banner_AdPosition;
    public BannerAdSize Banner_AdSize;

    public bool Opt_InterstitialAd;
    public bool Opt_RewardedAd;
    public bool Opt_RewardedInterstitialAd;
    public bool Opt_NativeOverlayAd;
    public bool Opt_AppOpenAd;

    [Space(15)]
    [Header("Auto Popup Section")]
    public bool OptForAutoPopupAds;

    [Space(15)]
    public bool Opt_AutoPopup_InterstitialAd;
    public bool Opt_AutoPopup_RewardedAd;
    public bool Opt_AutoPopup_RewardedInterstitialAd;


    [Header("Auto Popup TimeIn in seconds")]
    [Range(5, 900)]
    public float AutoPopup_InterstitialAd_TimeIn;
    [Range(5, 900)]
    public float AutoPopup_RewardedAd_TimeIn;
    [Range(5, 900)]
    public float AutoPopup_RewardedInterstitialAd_TimeIn;


    public static AdMobManager Instance { get; private set; }


    [Space(15)]
    public bool lastOnlineState;

    public static bool IsConnectedToInternet { get { return IsInternetAvailable(); } }
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


    #region Monobehaviour

    void Awake()
    {
      if (Instance == null) { Instance = this; }
      gameObject.name = this.GetType().Name;
      IsGoogle_MobileAds_Initilised = false;
      if (!ForceInternetRequired)
      {
        Destroy(FindAnyObjectByType<InternetMoniter>().gameObject);
      }
      if (!AsModuleEnabled) { return; }
      AppOpenAdController.Awake();
    }

    void Start()
    {
      DontDestroyOnLoad(gameObject);

      if (IsConnectedToInternet)
      {
        Initialize_AdMob();
      }
      lastOnlineState = IsConnectedToInternet;
    }

    private void Update()
    {
      if (lastOnlineState != IsConnectedToInternet)
      {
        if (IsConnectedToInternet)
        {
          if (!IsGoogle_MobileAds_Initilised)
          {
            Initialize_AdMob();
          }
          else
          {
            Validate_AdMob_Initilised();
          }
        }
        lastOnlineState = IsConnectedToInternet;
      }
    }



    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    void OnEnable()
    {
      SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
      SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
      AppOpenAdController.OnDestroy();
    }

    #endregion


    #region AdMob Initialize Section

    public void Initialize_AdMob()
    {
      if (!IsConnectedToInternet)
      {
        Debug.Log("Init_AdMob IsOffline return");
        return;
      }

      if (string.IsNullOrEmpty(AndroidAppId))
      {
        Debug.LogWarning($"AdMobAndroidAppId is null");
        return;
      }

      MobileAds.Initialize((InitializationStatus initstatus) =>
      {
        if (initstatus == null)
        {
          Debug.LogError("Google Mobile Ads initialization failed.");
          return;
        }
        Debug.Log("Google Mobile Ads initialization complete.");
        // Google Mobile Ads events are raised off the Unity Main thread. If you need to
        // access UnityEngine objects after initialization,
        // use MobileAdsEventExecsutor.ExecuteInUpdate(). For more information, see:
        // https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread
        IsGoogle_MobileAds_Initilised = true;
      });
      Invoke(nameof(Validate_AdMob_Initilised), 5);
    }

    void Validate_AdMob_Initilised()
    {
      Debug.Log("Validate_AdMob_Initilised");
      if (!IsGoogle_MobileAds_Initilised)
      {
        Invoke(nameof(Validate_AdMob_Initilised), 5);
      }
      else
      {
        Initilise_ADs();
        Apply_AutoPopup_Ads();
      }
    }

    void Initilise_ADs()
    {
      if (!IsConnectedToInternet) { return; }
      Debug.Log("Initilise_ADs");
      if (Opt_BannerAd && BannerViewController._bannerView == null)
      {
        BannerViewController.LoadAd();
      }

      if (Opt_InterstitialAd)
      {
        if (InterstitialAdController._interstitialAd == null || !InterstitialAdController._interstitialAd.CanShowAd())
        {
          InterstitialAdController.LoadAd();
        }
      }

      if (Opt_RewardedAd)
      {
        if (RewardedAdController._rewardedAd == null || !RewardedAdController._rewardedAd.CanShowAd())
        {
          RewardedAdController.LoadAd();
        }
      }

      if (Opt_RewardedInterstitialAd)
      {
        if (RewardedInterstitialAdController._rewardedInterstitialAd == null || !RewardedInterstitialAdController._rewardedInterstitialAd.CanShowAd())
        {
          RewardedInterstitialAdController.LoadAd();
        }
      }
      if (Opt_NativeOverlayAd)
      {
        if (NativeOverlayAdController._nativeOverlayAd == null)
        {
          NativeOverlayAdController.LoadAd();
        }
      }

      if (Opt_AppOpenAd)
      {
        if (AppOpenAdController._appOpenAd == null || !AppOpenAdController._appOpenAd.CanShowAd())
        {
          AppOpenAdController.LoadAd();
        }
      }
    }
    void Apply_AutoPopup_Ads()
    {
      Debug.Log("Apply_AutoPopup_Ads");
      CancelInvoke(nameof(Auto_Popup_InterstitialAd));
      CancelInvoke(nameof(Auto_Popup_RewardedAd));
      CancelInvoke(nameof(Auto_Popup_RewardedInterstitialAd));
      if (Opt_InterstitialAd && Opt_AutoPopup_InterstitialAd)
      {
        Invoke(nameof(Auto_Popup_InterstitialAd), AutoPopup_InterstitialAd_TimeIn);
      }
      if (Opt_RewardedAd && Opt_AutoPopup_RewardedAd)
      {
        Invoke(nameof(Auto_Popup_RewardedAd), AutoPopup_RewardedAd_TimeIn);
      }
      if (Opt_RewardedInterstitialAd && Opt_AutoPopup_RewardedInterstitialAd)
      {
        Invoke(nameof(Auto_Popup_RewardedInterstitialAd), AutoPopup_RewardedInterstitialAd_TimeIn);
      }
    }


    void Auto_Popup_InterstitialAd()
    {
      Debug.Log("Auto_Popup_InterstitialAd Invoked");
      Invoke(nameof(Auto_Popup_InterstitialAd), AutoPopup_InterstitialAd_TimeIn);
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      ShowInterstitialAd();
    }

    void Auto_Popup_RewardedAd()
    {
      Debug.Log("Auto_Popup_RewardedAd Invoked");
      Invoke(nameof(Auto_Popup_RewardedAd), AutoPopup_RewardedAd_TimeIn);
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      ShowRewardedAd();
    }

    void Auto_Popup_RewardedInterstitialAd()
    {
      Debug.Log("Auto_Popup_RewardedInterstitialAd Invoked");
      Invoke(nameof(Auto_Popup_RewardedInterstitialAd), AutoPopup_RewardedInterstitialAd_TimeIn);
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
      ShowRewardedInterstitialAd();
    }

    #region Banner Section

    public void ShowBannerAd()
    {
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
    void ShowInterstitialAd()
    {
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }
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
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }

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

    void ShowRewardedAd()
    {
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }

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
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }

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
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }

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
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }

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
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }


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
      if (!IsGoogle_MobileAds_Initilised || !IsConnectedToInternet) { return; }

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


    #endregion


    #region AdMob BannerViewController Section

    public class BannerViewController
    {
      public static bool IsBannerAd_OnScreen { get; set; }

      public static BannerView _bannerView;

      /// <summary>
      /// Creates a 320x50 banner at top of the screen.
      /// </summary>
      static void CreateBannerView()
      {
        Debug.Log("Creating banner view.");
        if (_bannerView != null)
        {
          DestroyAd();
        }

        switch (Instance.Banner_AdSize)
        {
          case BannerAdSize.Banner:
            _bannerView = new BannerView(Instance.BannerId, AdSize.Banner, Instance.Banner_AdPosition);
            break;
          case BannerAdSize.MediumRectangle:
            _bannerView = new BannerView(Instance.BannerId, AdSize.MediumRectangle, Instance.Banner_AdPosition);
            break;
          case BannerAdSize.IABBanner:
            _bannerView = new BannerView(Instance.BannerId, AdSize.IABBanner, Instance.Banner_AdPosition);
            break;
          case BannerAdSize.Leaderboard:
            _bannerView = new BannerView(Instance.BannerId, AdSize.Leaderboard, Instance.Banner_AdPosition);
            break;

        }
        // Create a 320x50 banner at top of the screen.
        //_bannerView = new BannerView(AdMobManager.Instance.BannerId, AdSize.IABBanner, AdPosition.Bottom);
        // Listen to events the banner may raise.
        ListenToAdEvents();

        Debug.Log("Banner view created.");
      }

      /// <summary>
      /// Creates the banner view and loads a banner ad.
      /// </summary>
      public static void LoadAd()
      {
        // Create an instance of a banner view first.
        if (_bannerView == null) { CreateBannerView(); }

        // Create our request used to load the ad.
        var adRequest = new AdRequest();
        // Send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
      }

      /// <summary>
      /// Shows the ad.
      /// </summary>
      public static void ShowAd()
      {
        if (_bannerView != null)
        {
          Debug.Log("Showing banner view.");
          _bannerView.Show();
        }
      }


      /// <summary>
      /// Hides the ad.
      /// </summary>
      public static void HideAd()
      {
        if (_bannerView != null)
        {
          Debug.Log("Hiding banner view.");
          _bannerView.Hide();
        }
      }

      /// <summary>
      /// Destroys the ad.
      /// When you are finished with a BannerView, make sure to call
      /// the Destroy() method before dropping your reference to it.
      /// </summary>
      public static void DestroyAd()
      {
        if (_bannerView != null)
        {
          Debug.Log("Destroying banner view.");
          _bannerView.Destroy();
          _bannerView = null;
        }

      }

      /// <summary>
      /// Logs the ResponseInfo.
      /// </summary>
      public static void LogResponseInfo()
      {
        if (_bannerView != null)
        {
          var responseInfo = _bannerView.GetResponseInfo();
          if (responseInfo != null)
          {
            Debug.Log(responseInfo);
          }
        }
      }

      /// <summary>
      /// Listen to events the banner may raise.
      /// </summary>
      static void ListenToAdEvents()
      {
        // Raised when an ad is loaded into the banner view.
        _bannerView.OnBannerAdLoaded += () =>
        {
          Debug.Log("Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
          ShowAd();
        };
        // Raised when an ad fails to load into the banner view.
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
          Debug.LogError("Banner view failed to load an ad with error : " + error);
          LoadAd();
        };
        // Raised when the ad is estimated to have earned money.
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
          Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        _bannerView.OnAdImpressionRecorded += () =>
        {
          Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _bannerView.OnAdClicked += () =>
        {
          Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
          Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
          Debug.Log("Banner view full screen content closed.");
          LoadAd();
        };
      }
    }

    #endregion


    #region AdMob InterstitialAdController Section

    public class InterstitialAdController : MonoBehaviour
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
        InterstitialAd.Load(Instance.InterstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
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
          LoadAd();
        };
      }
    }


    #endregion


    #region AdMob RewardedAdController Section

    public class RewardedAdController : MonoBehaviour
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
        RewardedAd.Load(AdMobManager.Instance.RewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
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
          LoadAd();
        };
      }
    }

    #endregion


    #region AdMob RewardedInterstitialAdController Section

    public class RewardedInterstitialAdController : MonoBehaviour
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
        RewardedInterstitialAd.Load(AdMobManager.Instance.RewardedInterstitialId, adRequest,
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
          LoadAd();
        };
      }
    }
    #endregion


    #region AdMob NativeOverlayAdController Section

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
        NativeOverlayAd.Load(AdMobManager.Instance.NativeOverlayId, adRequest, Option,
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

    #endregion


    #region AdMob AppOpen Section

    public class AppOpenAdController : MonoBehaviour
    {
      // App open ads can be preloaded for up to 4 hours.
      static readonly TimeSpan TIMEOUT = TimeSpan.FromHours(4);
      static DateTime _expireTime;

      public static bool IsAppOpenAdAvailable { get { return (_appOpenAd != null && _appOpenAd.CanShowAd()); } }

      public static AppOpenAd _appOpenAd;
      internal static void Awake()
      {
        // Use the AppStateEventNotifier to listen to application open/close events.
        // This is used to launch the loaded ad when we open the app.
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
      }
      internal static void OnDestroy()
      {
        // Always unlisten to events when complete.
        AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
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
        AppOpenAd.Load(AdMobManager.Instance.AppOpenId, adRequest, (AppOpenAd ad, LoadAdError error) =>
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

    #endregion
    public static bool IsInternetAvailable()
    {
#if UNITY_EDITOR
      return (UnityEngine.Device.Application.internetReachability != NetworkReachability.NotReachable);
#elif UNITY_ANDROID
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                // Get ConnectivityManager
                AndroidJavaObject connectivityManager = activity.Call<AndroidJavaObject>(
                    "getSystemService", "connectivity");

                if (connectivityManager != null)
                {
                    // Get active network info
                    AndroidJavaObject networkInfo = connectivityManager.Call<AndroidJavaObject>("getActiveNetworkInfo");

                    if (networkInfo != null)
                    {
                        bool connected = networkInfo.Call<bool>("isConnected");
                        return connected;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Internet check failed: " + e.Message);
        }
        return false;
#endif
    }

  }

}


#if UNITY_EDITOR
[CustomEditor(typeof(AdMobManager))]
public class MyScriptEditor : Editor
{
  public override void OnInspectorGUI()
  {
    AdMobManager myScript = (AdMobManager)target;

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
        if (property.name == "Banner_AdPosition")
        {
          if (myScript.Opt_BannerAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Banner_AdPosition"));
        }
        else if (property.name == "Banner_AdSize")
        {
          if (myScript.Opt_BannerAd)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Banner_AdSize"));
        }
        else if (property.name == "Opt_AutoPopup_InterstitialAd")
        {
          if (myScript.OptForAutoPopupAds)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Opt_AutoPopup_InterstitialAd"));
        }
        else if (property.name == "Opt_AutoPopup_RewardedAd")
        {
          if (myScript.OptForAutoPopupAds)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Opt_AutoPopup_RewardedAd"));
        }
        else if (property.name == "Opt_AutoPopup_RewardedInterstitialAd")
        {
          if (myScript.OptForAutoPopupAds)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Opt_AutoPopup_RewardedInterstitialAd"));
        }
        else if (property.name == "AutoPopup_InterstitialAd_TimeIn")
        {
          if (myScript.OptForAutoPopupAds)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoPopup_InterstitialAd_TimeIn"));
        }
        else if (property.name == "AutoPopup_RewardedAd_TimeIn")
        {
          if (myScript.OptForAutoPopupAds)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoPopup_RewardedAd_TimeIn"));
        }
        else if (property.name == "AutoPopup_RewardedInterstitialAd_TimeIn")
        {
          if (myScript.OptForAutoPopupAds)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoPopup_RewardedInterstitialAd_TimeIn"));
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

