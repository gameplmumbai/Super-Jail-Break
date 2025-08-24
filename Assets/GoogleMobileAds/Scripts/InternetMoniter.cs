using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InternetMoniter : MonoBehaviour
{
  [SerializeField]
  Sprite onlineIcon, offlineIcon;

  [SerializeField]
  TextMeshProUGUI TitleText, NoteText;

  [SerializeField]
  Canvas BlockCanvas;

  [SerializeField]
  Image BlockPanel;

  [SerializeField]
  RectTransform NotifyRect;

  [SerializeField]
  float NotifyRectHeight;

  public NetworkReachability _lastReachability;

  public Color FadeOut;
  public Color FadeIn;
  public float transitionDuration = 1.0f;

  private Color currentColor;
  private Color startLerpColor;
  private Color endLerpColor;
  private float elapsedTime = 0f;
  bool isOnline;
  private void Start()
  {
    BlockCanvas = GetComponent<Canvas>();
    NotifyRect = BlockPanel.transform.GetChild(0).GetComponent<RectTransform>();
    currentColor = BlockPanel.color;
    startLerpColor = FadeOut;
    endLerpColor = FadeIn;
    NotifyRectHeight = NotifyRect.rect.height;
  }

  void Update()
  {
#if UNITY_EDITOR
    if (_lastReachability != UnityEngine.Device.Application.internetReachability)
    {
      Time.timeScale = (UnityEngine.Device.Application.internetReachability == NetworkReachability.NotReachable) ? 0 : 1;
      elapsedTime = 0f;
      _lastReachability = UnityEngine.Device.Application.internetReachability;
    }

#else
    if (_lastReachability != Application.internetReachability)
    {
		  Time.timeScale = (Application.internetReachability == NetworkReachability.NotReachable) ? 0 : 1;
      elapsedTime = 0f;
      _lastReachability = Application.internetReachability;
    }
#endif

    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / transitionDuration);

    isOnline = _lastReachability != NetworkReachability.NotReachable;
    BlockPanel.color = Color.Lerp(BlockPanel.color, (isOnline ? FadeOut : FadeIn), t);

    NotifyRect.anchoredPosition = new Vector2(NotifyRect.anchoredPosition.x, Mathf.Lerp(NotifyRect.anchoredPosition.y, (isOnline ? NotifyRectHeight : 0), t));
    BlockCanvas.enabled = (BlockPanel.color.a > 0);
  }



}
