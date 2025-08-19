using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;


public class Share : MonoBehaviour {

	public string subject = "Install this amazing Game";
	public string Link = "https://www.sellmyapp.com/downloads/penguin-io-addictive-game/";


    public void ShareNow() {
        StartCoroutine(OniOSTextSharingClick());
    }

    IEnumerator OniOSTextSharingClick()
    {
        yield return new WaitForEndOfFrame();

        new NativeShare().SetSubject(subject).SetText(Link).Share();



    }

}
