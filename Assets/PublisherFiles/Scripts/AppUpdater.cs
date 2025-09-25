using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppUpdater : MonoBehaviour
{
  [SerializeField] RectTransform panel;
  //[SerializeField] RectTransform PortraitUI, LandscapeUI;
  [SerializeField] Vector2 hidepos, visiblepos;
  float elapsed;
  [SerializeField] float slideInDuration;
  [SerializeField] Sprite appicon;
  [SerializeField] Image AppImage;

  private void Awake()
  {
    switch (Screen.orientation)
    {
      case ScreenOrientation.Portrait:
      case ScreenOrientation.PortraitUpsideDown:
        panel.rect.Set(0, 0, Screen.width, Screen.height / 2);
        //panel.sizeDelta = new Vector2(Screen.width, Screen.height / 2);
        break;
      case ScreenOrientation.LandscapeLeft:
      case ScreenOrientation.LandscapeRight:
        panel.rect.Set(0, 0, Screen.width, Screen.height);
        //panel.sizeDelta = new Vector2(Screen.width, Screen.height);
        break;
    }
    panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
    panel.gameObject.SetActive(true);
    panel.anchoredPosition = hidepos;
  }
  void Start()
  {

  }
  private void Update()
  {

    elapsed += Time.deltaTime;
    float t = Mathf.Clamp01(elapsed / slideInDuration);
    float ease = t * t;
    panel.anchoredPosition = Vector2.Lerp(hidepos, visiblepos, ease);
  }

  // Update is called once per frame
  public void UpdateApp()
  {
    Debug.Log("UpdateApp");
    try
    {
      Application.OpenURL("market://details?id=" + Application.identifier);
      Debug.Log("try");
    }
    catch
    {
      Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
      Debug.Log("catch");
    }

  }


}

