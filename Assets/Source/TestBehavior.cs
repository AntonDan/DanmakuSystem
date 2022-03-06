using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Projectiles;
using Unity.Entities;
using Sirenix.OdinInspector;
using Unity.Transforms;

public class TestBehavior : MonoBehaviour
{
	public GameObject _projectilePrefab;

	private Entity _projectileEntity;
	private EntityManager _entityManager;
	private BlobAssetStore _blobAssetStore;

	private CircularProjectileEmitter _emitter = new CircularProjectileEmitter(Vector2.zero, 0, 0, 0, new ProjectileCircleSpawnerConfig(10, 3.0f, 4, 3.0f));
	private CircularProjectileEmitter _subemitter = new CircularProjectileEmitter(Vector2.zero, 0, 0, 20, new ProjectileCircleSpawnerConfig(5, 0.2f, 8, 0.5f));

	private void Awake()
	{
		_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		_blobAssetStore = new BlobAssetStore();
		GameObjectConversionSettings conversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
		_projectileEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(_projectilePrefab, conversionSettings);
	}

	void Start()
	{
		StartCoroutine(TestRoutine());
	}

	private void OnDestroy()
	{
		_blobAssetStore.Dispose();
	}

	IEnumerator TestRoutine()
	{
		yield return new WaitForSeconds(1.0f);
		_emitter.Of(_subemitter).Of(_projectileEntity).Fire();
	}

	// TODO PROJECTILE FIRING PATTERN FEATURES 
	// 1) Combine shapes (circle, square, hexagon, arc/cone, line, )
	// 2) Rotate around self or specified pivot (can be done by combining shapes?)
	// 3) Bursts (can be done by combining shapes?)
	// 4) Projectile trajectory (straight, homing, sine, curve, circle). Firing speed combines 
}
