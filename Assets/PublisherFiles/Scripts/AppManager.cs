using Google.Play.AppUpdate;
using Google.Play.Common;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
  public static AppManager Instance { get; private set; }

  GameObject ShowAppUpdatePopup;

  [SerializeField]
  bool ForceInternetRequired;

  [SerializeField]
  bool EnableIAPModule;

  GameObject InternetRequiredPanel;

  public static event UnityAction<bool> onInterNetChange;
  [Space(15)]
  [SerializeField]
  bool lastOnlineState;

  [SerializeField]
  UpdateAvailability updateAvailability;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    DontDestroyOnLoad(gameObject);
    lastOnlineState = IsInternetAvailable();
  }

  private void Start()
  {
    SpawnInternetblocker();
    CheckForUpdate();
  }

  void SpawnInternetblocker()
  {
    GameObject blocker = Resources.Load<GameObject>("Internetblocker");
    if (blocker != null)
    {
      Instantiate(blocker, gameObject.transform);
    }
    else
    {
      Debug.LogError("UIPopup_AppUpdate Prefab not found!");
    }
  }

  private void Update()
  {
    if (lastOnlineState != IsInternetAvailable())
    {
      lastOnlineState = IsInternetAvailable();
      onInterNetChange?.Invoke(lastOnlineState);
    }
  }
  public void CheckForUpdate()
  {
#if UNITY_ANDROID && !UNITY_EDITOR
    StartCoroutine(ICheckForUpdate());
#endif
  }

  IEnumerator ICheckForUpdate()
  {
    AppUpdateManager appUpdateManager = new AppUpdateManager();

    PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();
    yield return appUpdateInfoOperation;
    if (appUpdateInfoOperation.IsSuccessful)
    {
      var appUpdateInfoResult = appUpdateInfoOperation.GetResult();
      updateAvailability = appUpdateInfoResult.UpdateAvailability;
      if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
      {
        Debug.Log($"appUpdateInfoResult.UpdateAvailability:{UpdateAvailability.UpdateAvailable}");
        ForceAppUpdate();
      }
    }
    else
    {
      updateAvailability = UpdateAvailability.UpdateNotAvailable;
      Debug.Log($"NO update available");
      // Log appUpdateInfoOperation.Error.
    }
  }



  string _toastString;
  string _input;
  AndroidJavaObject _currentActivity;
  AndroidJavaClass _unityPlayer;
  AndroidJavaObject _context;

  void StartToast()
  {
    if (Application.platform == RuntimePlatform.Android)
    {
      _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
      _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
      _context = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
    }
  }


  public static void ShowToastOnUiThread(string toastString)
  {
    if (Application.platform != RuntimePlatform.Android)
    {
      return;
    }

    Instance._toastString = toastString;
    Instance._currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(Instance.ShowToast));
  }

  void ShowToast()
  {
    Debug.Log(this + ": Running on UI thread");

    var t = new AndroidJavaClass("android.widget.Toast");
    var javaString = new AndroidJavaObject("java.lang.String", _toastString);
    var toast = t.CallStatic<AndroidJavaObject>("makeText", _context, javaString, t.GetStatic<int>("LENGTH_SHORT"));
    toast.Call("show");
  }

  public void ForceAppUpdate()
  {
    GameObject ShowAppUpdatePopup = Resources.Load<GameObject>("UIPopup_AppUpdate");
    if (ShowAppUpdatePopup != null)
    {
      for (int i = 0; i < SceneManager.sceneCount; i++)
      {
        foreach (var item in SceneManager.GetSceneAt(i).GetRootGameObjects().ToArray())
        {
          if (item.GetComponent<Camera>() == null)
          {
            DestroyImmediate(item);
          }
        }
      }
      Scene dnd = SceneManager.GetActiveScene();
      foreach (var item in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToArray())
      {
        if (item.scene.buildIndex == -1)
        {
          dnd = item.scene;
          break;
        }
      }

      foreach (var item in dnd.GetRootGameObjects().ToArray())
      {
        if (item.GetComponent<AppManager>() == null)
        {
          Destroy(item);
        }
      }
      Instantiate(ShowAppUpdatePopup, gameObject.transform);
    }
    else
    {
      Debug.LogError("UIPopup_AppUpdate Prefab not found!");
    }
  }


  public void ShareAppLink()
  {
    // Replace with your Play Store link
    string appUrl = "https://play.google.com/store/apps/details?id=" + Application.identifier;
    Debug.Log($"ShareAppLink: {appUrl}");

    // Create intent
    AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
    AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

    // Set action to SEND
    intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

    // Put the text (link) to share
    intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), appUrl);

    // Set type to plain text
    intentObject.Call<AndroidJavaObject>("setType", "text/plain");

    // Get current Unity activity
    AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

    // Create chooser
    AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>(
        "createChooser", intentObject, "Share App");

    // Start activity
    currentActivity.Call("startActivity", chooser);
  }
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



#if UNITY_EDITOR
[CustomEditor(typeof(AppManager))]
public class AppManagerEditor : Editor
{
  public override void OnInspectorGUI()
  {
    AppManager myScript = (AppManager)target;

    SerializedProperty property = serializedObject.GetIterator();
    bool expanded = property.NextVisible(true);
    if (GUILayout.Button("Fake App Update"))
    {
      myScript.ForceAppUpdate();
    }

    while (expanded)
    {

      if (property.name != "m_Script")
      {
        EditorGUILayout.PropertyField(property, true);
      }

      expanded = property.NextVisible(false);
    }


    serializedObject.ApplyModifiedProperties();

  }

}
#endif
