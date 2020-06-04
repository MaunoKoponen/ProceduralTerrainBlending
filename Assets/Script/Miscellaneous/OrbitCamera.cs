using UnityEngine;
using System.Collections;

public class OrbitCamera : MonoBehaviour
{
	public Vector3 LookAtPosition = Vector3.zero;

	public float Zoom = 5;
	public float ZoomSpeed = 3;
	public float MinZoom = 1;
	public float MaxZoom = 10;

	public float ZoomOrthographicSizeFactor = 1;
	public float ZoomDistanceFactor = 10;

	public float RotatePitchFactor = 3;
	public float RotateYawFactor = 3;

	private Vector2 _pressedMousePosition;
	private Vector3 _pressedGroundPosition;
	private Vector3 _pressedLookAtPosition;

	// Update is called once per frame
	void Update()
	{
		var camera = this.GetComponent<Camera>();

		var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");

		if (Mathf.Abs(mouseScrollWheel) > float.Epsilon)
		{
			Zoom = Mathf.Clamp(Zoom + ZoomSpeed * -mouseScrollWheel, MinZoom, MaxZoom);
		}

		if (Input.GetMouseButtonDown(2))
		{
			_pressedMousePosition = Input.mousePosition;
			var ray = camera.ScreenPointToRay(_pressedMousePosition);
			_pressedGroundPosition = ray.origin + ray.direction * -ray.origin.y / ray.direction.y;
			_pressedLookAtPosition = LookAtPosition;
		}

		if (Input.GetMouseButton(2))
		{
			var oldRay = camera.ScreenPointToRay(_pressedMousePosition);
			var oldGroundPosition = oldRay.origin + oldRay.direction * -oldRay.origin.y / oldRay.direction.y;
			var newRay = camera.ScreenPointToRay(Input.mousePosition);
			var newGroundPosition = newRay.origin + newRay.direction * -newRay.origin.y / newRay.direction.y;
			var offset = newGroundPosition - oldGroundPosition;
			LookAtPosition = _pressedLookAtPosition - offset;
		}
			
		if (Input.GetMouseButton(0))
		{
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");

			var euler = transform.localEulerAngles;
			euler += new Vector3(mouseY * RotatePitchFactor, mouseX * RotateYawFactor, 0);
			euler.x = Mathf.Clamp(euler.x, 5f, 85f);
			transform.localEulerAngles = euler;
   		}

		transform.position = LookAtPosition - transform.localRotation * Vector3.forward * Zoom * ZoomDistanceFactor;
		camera.orthographicSize = Zoom * ZoomOrthographicSizeFactor;
	}
}
