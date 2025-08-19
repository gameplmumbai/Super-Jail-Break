using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AdsManager : MonoBehaviour
{
  public bool AsModuleEnabled;

  public string AndroidAppId, BannerId, InterstitialId, RewardedId, RewardedInterstitialId, NativeOverlayId, AppOpenId;


  public float AdMobObserverTickRate;

  [Header("Opt For AdMob Types")]
  public bool Opt_BannerAd;
  public bool Opt_InterstitialAd;
  public bool Opt_RewardedAd;
  public bool Opt_RewardedInterstitialAd;
  public bool Opt_AppOpenAd;


  [Header("Opt For AutoPopup")]
  public bool Opt_AutoPopup_InterstitialAd;
  public bool Opt_AutoPopup_RewardedAd;
  public bool Opt_AutoPopup_RewardedInterstitialAd;


  [Header("First Popup TimeIn")]
  public float FirstPopup_InterstitialAd_TimeIn;
  public float FirstPopup_RewardedAd_TimeIn;
  public float FirstPopup_RewardedInterstitialAd_TimeIn;


  [Header("Auto Popup TimeIn")]
  public float AutoPopup_InterstitialAd_TimeIn;
  public float AutoPopup_RewardedAd_TimeIn;
  public float AutoPopup_RewardedInterstitialAd_TimeIn;


  private NetworkReachability _lastReachability;

  public static AdsManager Instance { get; private set; }

  #region Monobehaviour

  void Awake()
  {
    if (Instance == null) { Instance = this; }
    gameObject.name = this.GetType().Name;
    _lastReachability = Application.internetReachability;
    if (!AsModuleEnabled) { return; }
    Init_Popup_TimeIns();
    AppOpenAdController.Awake();
  }

  void Init_Popup_TimeIns()
  {
    FirstPopup_InterstitialAd_TimeIn = (FirstPopup_InterstitialAd_TimeIn <= 0) ? 10 : FirstPopup_InterstitialAd_TimeIn;
    FirstPopup_RewardedAd_TimeIn = (FirstPopup_RewardedAd_TimeIn <= 0) ? 10 : FirstPopup_RewardedAd_TimeIn;
    FirstPopup_RewardedInterstitialAd_TimeIn = (FirstPopup_RewardedInterstitialAd_TimeIn <= 0) ? 10 : FirstPopup_RewardedInterstitialAd_TimeIn;

    AutoPopup_InterstitialAd_TimeIn = (AutoPopup_InterstitialAd_TimeIn <= 0) ? 30 : AutoPopup_InterstitialAd_TimeIn;
    AutoPopup_RewardedAd_TimeIn = (AutoPopup_RewardedAd_TimeIn <= 0) ? 30 : AutoPopup_RewardedAd_TimeIn;
    AutoPopup_RewardedInterstitialAd_TimeIn = (AutoPopup_RewardedInterstitialAd_TimeIn <= 0) ? 30 : AutoPopup_RewardedInterstitialAd_TimeIn;

  }


  void Start()
  {
    DontDestroyOnLoad(gameObject);
    if (!AsModuleEnabled) { return; }


    Init_AdMob();
  }

  void Update()
  {
    NetworkReachability currentReachability = Application.internetReachability;

    if (currentReachability != _lastReachability)
    {
      Debug.Log("Internet Reachability Changed! From: " + _lastReachability + " To: " + currentReachability);
      // Call custom methods or trigger events here based on the new reachability
      HandleInternetReachabilityChange(currentReachability);
      _lastReachability = currentReachability; // Update for next check
    }
  }

  void HandleInternetReachabilityChange(NetworkReachability newReachability)
  {
    if (newReachability != NetworkReachability.NotReachable)
    {
      //Init_AdMob();

      Debug.Log("Init AdMob Reinit");
    }
    switch (newReachability)
    {
      case NetworkReachability.NotReachable:
        Debug.Log("No internet connection.");
        // Perform actions for no connection (e.g., disable online features)
        break;
      case NetworkReachability.ReachableViaCarrierDataNetwork:
        Debug.Log("Internet reachable via carrier data network.");
        // Perform actions for carrier data network
        break;
      case NetworkReachability.ReachableViaLocalAreaNetwork:
        Debug.Log("Internet reachable via local area network (Wi-Fi/Ethernet).");
        // Perform actions for local area network
        break;
    }
  }

  void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    //Debug.LogWarning(" asdas dasd asdScene loaded: " + scene.name);
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
    Debug.Log($"MobileAds destroyed");
    AppOpenAdController.OnDestroy();
  }

  #endregion


  #region AdMob

  public void Init_AdMob()
  {
    if (string.IsNullOrEmpty(AndroidAppId))
    {
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
      InvokeRepeating(nameof(AdView_Observer), 1, AdMobObserverTickRate);
      Apply_AutoPopup_Ads();
    });

  }


  void AdView_Observer()
  {
    if (Opt_BannerAd && BannerViewController._bannerView == null)
    {
      BannerViewController.LoadAd();
    }

    if (Opt_InterstitialAd && !IsInterstitialAdAvailable)
    {
      InterstitialAdController.LoadAd();
    }

    if (Opt_RewardedAd && !IsRewardedAdVideoAvailable)
    {
      RewardedAdController.LoadAd();
    }

    if (Opt_RewardedInterstitialAd && !IsRewardedInterstitialAdVideoAvailable)
    {
      RewardedInterstitialAdController.LoadAd();
    }

    if (Opt_AppOpenAd && !IsAppOpenAvailable)
    {
      AppOpenAdController.LoadAd();
    }
  }


  public void Apply_AutoPopup_Ads()
  {
    if (Opt_InterstitialAd && Opt_AutoPopup_InterstitialAd)
    {
      Debug.LogWarning("Apply_AutoPopup_Ads Opt_InterstitialAd");
      InvokeRepeating(nameof(ShowInterstitialAd), FirstPopup_InterstitialAd_TimeIn, AutoPopup_InterstitialAd_TimeIn);
    }
    if (Opt_RewardedAd && Opt_AutoPopup_RewardedAd)
    {
      InvokeRepeating(nameof(ShowRewardedAd), FirstPopup_RewardedAd_TimeIn, AutoPopup_RewardedAd_TimeIn);
    }
    if (Opt_RewardedInterstitialAd && Opt_AutoPopup_RewardedInterstitialAd)
    {
      InvokeRepeating(nameof(ShowRewardedInterstitialAd), FirstPopup_RewardedInterstitialAd_TimeIn, AutoPopup_RewardedInterstitialAd_TimeIn);
    }
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
  public bool IsInterstitialAdAvailable { get { return InterstitialAdController.IsInterstitialAdAvailable; } }
  public void ShowInterstitialAd()
  {
    Debug.Log("ShowInterstitialAd");
    if (!Opt_InterstitialAd && !IsInterstitialAdAvailable) { return; }
    InterstitialAdController.ShowAd();
  }
  public void ShowInterstitialAd_callback(Action<bool> callback)
  {
    if (!Opt_InterstitialAd && !IsInterstitialAdAvailable) { return; }
    InterstitialAdController.ShowAd(callback);
  }

  #endregion

  #region Rewarded Section

  public bool IsRewardedAdVideoAvailable { get { return RewardedAdController.IsRewardVideoAvailable; } }

  public void ShowRewardedAd()
  {
    if (!Opt_RewardedAd && !IsRewardedAdVideoAvailable) { return; }
    RewardedAdController.ShowAd();
  }

  public void ShowRewardedAd_callback(Action<bool> callback)
  {
    if (!Opt_RewardedAd && !IsRewardedAdVideoAvailable) { return; }
    RewardedAdController.ShowAd_Call(callback);
  }

  #endregion

  #region RewardedInterstitial Section

  public bool IsRewardedInterstitialAdVideoAvailable { get { return RewardedInterstitialAdController.IsRewardedInterstitialAdAvailable; } }

  public void ShowRewardedInterstitialAd()
  {
    if (!Opt_RewardedInterstitialAd && !IsRewardedInterstitialAdVideoAvailable) { return; }
    RewardedInterstitialAdController.ShowAd();
  }

  public void ShowRewardedInterstitialAd_callback(Action<bool> callback)
  {
    if (!Opt_RewardedInterstitialAd && !IsRewardedInterstitialAdVideoAvailable) { return; }
    RewardedInterstitialAdController.ShowAd(callback);
  }
  #endregion

  #region App Open Section

  public bool IsAppOpenAvailable { get { return AppOpenAdController.IsAppOpenAdAvailable; } }

  public void LoadAppOpenAdAd()
  {
    if (!Opt_AppOpenAd && !IsAppOpenAvailable)
    {
      return;
    }
    AppOpenAdController.LoadAd();
  }

  #endregion


  #endregion

  #region AdMob BannerViewController Section

  public class BannerViewController
  {
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

      // Create a 320x50 banner at top of the screen.
      _bannerView = new BannerView(AdsManager.Instance.BannerId, AdSize.IABBanner, AdPosition.Bottom);
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

      // Inform the UI that the ad is not ready.
      //AdLoadedStatus?.SetActive(false);
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

        // Inform the UI that the ad is ready.
        //AdLoadedStatus?.SetActive(true);
      };
      // Raised when an ad fails to load into the banner view.
      _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
      {
        Debug.LogError("Banner view failed to load an ad with error : " + error);
        DestroyAd();
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
      };
    }
  }

  #endregion


  #region AdMob InterstitialAdController Section

  public class InterstitialAdController : MonoBehaviour
  {
    //public static InterstitialAdController Instance { get; private set; }

    public static bool IsInterstitialAdAvailable { get { return (_interstitialAd != null && _interstitialAd.CanShowAd()); } }
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
      InterstitialAd.Load(AdsManager.Instance.InterstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
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

      // Inform the UI that the ad is not ready.
      //AdLoadedStatus?.SetActive(false);
    }
    public static void ShowAd(Action<bool> callback)
    {
      if (_interstitialAd != null && _interstitialAd.CanShowAd())
      {
        Debug.Log("Showing interstitial ad.");
        _interstitialAd.Show();
        callback(true);
      }
      else
      {
        Debug.LogWarning("Interstitial ad is not ready yet.");
        callback(false);
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
      };
      // Raised when the ad failed to open full screen content.
      ad.OnAdFullScreenContentFailed += (AdError error) =>
      {
        Debug.LogError("Interstitial ad failed to open full screen content with error : "
                  + error);
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
      RewardedAd.Load(AdsManager.Instance.RewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
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

    public static void ShowAd_Call(Action<bool> rewardCallback)
    {
      if (_rewardedAd != null && _rewardedAd.CanShowAd())
      {
        Debug.Log("Showing rewarded ad.");
        _rewardedAd.Show((Reward reward) =>
        {
          Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                          reward.Amount,
                                          reward.Type));
          rewardCallback(true);
        });
      }
      else
      {
        Debug.LogWarning("Rewarded ad is not ready yet.");
        rewardCallback(false);
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
        Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                  adValue.Value,
                  adValue.CurrencyCode));
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
      RewardedInterstitialAd.Load(AdsManager.Instance.RewardedInterstitialId, adRequest,
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

      // Inform the UI that the ad is not ready.
      //AdLoadedStatus?.SetActive(false);
    }


    /// <summary>
    /// Shows the ad.
    /// </summary>
    public static void ShowAd(Action<bool> rewardCallback)
    {
      if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
      {
        _rewardedInterstitialAd.Show((Reward reward) =>
        {
          Debug.Log("Rewarded interstitial ad rewarded : " + reward.Amount);
        });
        rewardCallback(true);
      }
      else
      {
        Debug.LogWarning("Rewarded interstitial ad is not ready yet.");
        rewardCallback(false);
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
      };
      // Raised when the ad failed to open full screen content.
      ad.OnAdFullScreenContentFailed += (AdError error) =>
      {
        Debug.LogError("Rewarded interstitial ad failed to open full screen content" +
                             " with error : " + error);
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

    static NativeOverlayAd _nativeOverlayAd;

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
      NativeOverlayAd.Load(AdsManager.Instance.NativeOverlayId, adRequest, Option,
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
      };
      // Raised when the ad closed full screen content.
      ad.OnAdFullScreenContentClosed += () =>
      {
        Debug.Log("Native Overlay ad full screen content closed.");
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
      AppOpenAd.Load(AdsManager.Instance.AppOpenId, adRequest, (AppOpenAd ad, LoadAdError error) =>
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

        // It may be useful to load a new ad when the current one is complete.
      };
      // Raised when the ad failed to open full screen content.
      ad.OnAdFullScreenContentFailed += (AdError error) =>
      {
        Debug.LogError("App open ad failed to open full screen content with error : "
                              + error);
      };
    }
  }

  #endregion
}
