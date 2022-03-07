using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SharedParticleRenderingConfig : MonoBehaviour
{
	public static SharedParticleRenderingConfig Instance
	{
		get; set;
	}

	public Material projectileMaterial;

	void Start()
	{
		if (Instance != null)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}
}
