using GoogleMobileAds.Api;
using GoogleMobileAds.Samples;
using System;
using UnityEngine;

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

		switch (AdsController.Instance.Banner_AdSize)
		{
			case AdsController.BannerAdSize.Banner:
				_bannerView = new BannerView(AdsController.Instance.BannerId, AdSize.Banner, AdsController.Instance.Banner_AdPosition);
				break;
			case AdsController.BannerAdSize.SmartBanner:
				var adSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
				_bannerView = new BannerView(AdsController.Instance.BannerId, adSize, AdsController.Instance.Banner_AdPosition);
				break;
			case AdsController.BannerAdSize.MediumRectangle:
				_bannerView = new BannerView(AdsController.Instance.BannerId, AdSize.MediumRectangle, AdsController.Instance.Banner_AdPosition);
				break;
			case AdsController.BannerAdSize.IABBanner:
				_bannerView = new BannerView(AdsController.Instance.BannerId, AdSize.IABBanner, AdsController.Instance.Banner_AdPosition);
				break;
			case AdsController.BannerAdSize.Leaderboard:
				_bannerView = new BannerView(AdsController.Instance.BannerId, AdSize.Leaderboard, AdsController.Instance.Banner_AdPosition);
				break;

		}
		// Create a 320x50 banner at top of the screen.
		//_bannerView = new BannerView(Instance.BannerId, AdSize.IABBanner, AdPosition.Bottom);
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
			//LoadAd();
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
