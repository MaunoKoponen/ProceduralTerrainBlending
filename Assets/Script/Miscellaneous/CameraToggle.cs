using UnityEngine;
using System.Collections;

public class CameraToggle : MonoBehaviour
{
	public OrbitCamera OrbitCamera;
	public TurnCamera TurnCamera;

	public bool AutoCamera = true;

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 100, 20), AutoCamera ? "Free cam" : "Auto cam"))
		{
			AutoCamera = !AutoCamera;
			if (AutoCamera)
			{
				transform.position = TurnCamera.startPos;
				TurnCamera.enabled = true;
				OrbitCamera.enabled = false;
			}
			else
			{
				TurnCamera.enabled = false;
				OrbitCamera.enabled = true;
			}
		}
	}
}
