using UnityEngine;

public class AdObject : MonoBehaviour
{
  private void Awake()
  {
    DontDestroyOnLoad(gameObject);
  }
}
