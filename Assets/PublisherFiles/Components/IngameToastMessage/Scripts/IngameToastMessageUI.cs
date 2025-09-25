using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static IngameToastMessageManager;

public class IngameToastMessageUI : MonoBehaviour
{
  public float slideDuration;
  public float stayTime;

  [SerializeField]
  Vector2 hiddenPos;

  [SerializeField]
  Vector2 visiblePos;

  [SerializeField]
  RectTransform rt;

  [SerializeField]
  TextMeshProUGUI messagetext;

  [SerializeField]
  Image Bg;

  [SerializeField]
  Color colorSuccess, colorWarning, colorFailed;


  private void Awake()
  {
    hiddenPos = rt.anchoredPosition;
  }
  public void ShowToastMessage(string Message)
  {
    messagetext.text = Message;
    StartCoroutine(AnimateUI(Message));
  }

  IEnumerator AnimateUI(string Message)
  {
    Debug.LogWarning($"slideDuration:{slideDuration}");
    float elapsed = 0;

    yield return new WaitUntil(() =>
    {
      elapsed += Time.deltaTime;
      float t = (elapsed / slideDuration);
      rt.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, t);
      return (t >= 1);
    });
    Debug.Log($"In");
    elapsed = 0;
    yield return new WaitForSeconds(stayTime);
    yield return new WaitUntil(() =>
      {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / slideDuration);
        rt.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, t);
        return (t >= 1);
      });
    Debug.Log($"out");
    Destroy(gameObject);


    yield return null;


  }
}

