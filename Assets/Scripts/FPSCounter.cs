using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{

    private float FPS;
    private GUIStyle style = new GUIStyle();

	void Start ()
    {
        StartCoroutine(Counter());
        style.fontSize = 20;
	}

    void OnGUI()
    {
        GUI.Label(new Rect(8, 8, 300, 80), "FPS: " + FPS.ToString(), style);
    }
	
    IEnumerator Counter()
    {
        while(true)
        {
            FPS = 1.0f / Time.deltaTime;
            yield return new WaitForSeconds(1);
        }
    }
}
