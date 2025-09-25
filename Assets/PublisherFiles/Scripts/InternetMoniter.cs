using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InternetMoniter : MonoBehaviour
{
  [SerializeField] RectTransform panel;
  [SerializeField] float slideInDuration;
  [SerializeField] Canvas canvas;
  [SerializeField]
  private void Awake()
  {
    canvas = GetComponent<Canvas>();
    switch (Screen.orientation)
    {
      case ScreenOrientation.Portrait:
      case ScreenOrientation.PortraitUpsideDown:
        panel.rect.Set(0, 0, Screen.width, Screen.height);
        break;
      case ScreenOrientation.LandscapeLeft:
      case ScreenOrientation.LandscapeRight:
        panel.rect.Set(0, 0, Screen.width, Screen.height);
        break;
    }
    panel.gameObject.SetActive(true);
  }

  private void Start()
  {
    HandleInternetReachibilityChanged(AppManager.IsInternetAvailable());
  }

  private void Update()
  {

  }


  private void OnEnable()
  {
    AppManager.onInterNetChange += HandleInternetReachibilityChanged;
  }

  private void OnDisable()
  {
    AppManager.onInterNetChange -= HandleInternetReachibilityChanged;
  }

  void HandleInternetReachibilityChanged(bool state)
  {
    Debug.Log($"state:{state}");
    canvas.enabled = !state;
  }

}
