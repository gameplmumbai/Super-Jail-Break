using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class IngameToastMessageManager : MonoBehaviour
{
	public static IngameToastMessageManager Instance { get; private set; }
	public enum PopupType { Success, Warning, Failed }

	static IngameToastMessageUI ingameToastMessagePrefab;
	[SerializeField]
	RectTransform pivot;
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}

	}
	private void Start()
	{
		ingameToastMessagePrefab = Resources.Load<IngameToastMessageUI>("IngameToastMessageUI");
	}
	public void ShowMessage(string message)
	{
		Instantiate(ingameToastMessagePrefab, pivot).ShowToastMessage(message);
	}

}


#if UNITY_EDITOR
[CustomEditor(typeof(IngameToastMessageManager))]
public class IngameToastMessageManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		IngameToastMessageManager myScript = (IngameToastMessageManager)target;

		SerializedProperty property = serializedObject.GetIterator();
		bool expanded = property.NextVisible(true);
		if (GUILayout.Button("Moke ToastMessage"))
		{
			myScript.ShowMessage("demo message");
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