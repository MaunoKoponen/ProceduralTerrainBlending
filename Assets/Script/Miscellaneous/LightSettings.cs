using UnityEngine;
using System.Collections;

public class LightSettings : MonoBehaviour
{
	public Material SkyboxMaterial;
	public Color CameraBackColor;
	public Color LightColor = Color.white;
	public float LightIntensity = 1;
	public Color AmbientColor = Color.black;

	public float LightYaw = 30f;
	public float LightPitch = 15f;

	public float ShadowStrength = 1;
	public float ShadowBias = 0.3f;

	public bool Fog = true;
	public Color FogColor = Color.blue;
	public float FogStartDistance = 100;
	public float FogEndDistance = 1000;

	public void Apply()
	{
		var lightObject = GameObject.Find("Light");
		Light light = null;
		if (lightObject != null)
			light = lightObject.GetComponent<Light>();
		if (light == null)
			light = FindObjectOfType<Light>();
		light.transform.rotation = Quaternion.Euler(LightPitch, LightYaw, 0);
		light.color = LightColor;
		light.intensity = LightIntensity;
		light.shadowBias = ShadowBias;
		light.shadowStrength = ShadowStrength;

		var camera = FindObjectOfType<Camera>();
		if (camera != null)
			camera.backgroundColor = CameraBackColor;

		RenderSettings.skybox = SkyboxMaterial;
		RenderSettings.ambientLight = AmbientColor;
		RenderSettings.fog = Fog;
		RenderSettings.fogColor = FogColor;
		RenderSettings.fogStartDistance = FogStartDistance;
		RenderSettings.fogEndDistance = FogEndDistance;
		RenderSettings.fogMode = FogMode.Linear;
	}

}
