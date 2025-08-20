using System.Collections;
using UnityEngine;


public class Share : MonoBehaviour
{

  public string subject = "Install this amazing Game";
  public string Link = "https://www.gameinstituteindia.com";


  public void ShareNow()
  {
    StartCoroutine(OniOSTextSharingClick());
  }

  IEnumerator OniOSTextSharingClick()
  {
    yield return new WaitForEndOfFrame();

    new NativeShare().SetSubject(subject).SetText(Link).Share();



  }

}
