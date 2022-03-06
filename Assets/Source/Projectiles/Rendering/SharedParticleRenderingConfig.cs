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

		string str = "";
		foreach (var property in typeof(TerrainData).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			str += property + "\n";
		}

		foreach (var field in typeof(TerrainData).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			str += field + "\n";
		}

		foreach (var method in typeof(TerrainData).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			str += method + "\n";
		}
		print(str);
	}
}
