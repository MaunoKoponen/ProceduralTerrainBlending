using UnityEngine;
using System.Collections;

public class TurnCamera : MonoBehaviour {

	public double angle = 0;
	public Vector3 center;
	public Vector3 startPos;
	public float height;
	private bool activate = true;

	// Use this for initialization
	void Start () {
		if (activate) {
			transform.position = startPos;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (activate) {
			transform.RotateAround(center, Vector3.up, (float)angle * Time.deltaTime);
			transform.LookAt(center);
			transform.position = new Vector3(transform.position.x, height, transform.position.z);
		}
	}
}
